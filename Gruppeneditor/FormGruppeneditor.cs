/*
 * Gruppeneditor
 * 
 * (c) Jonathan Vogt
 * 
 * License GPL
 * 
 */


using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;

namespace Gruppeneditor
{
    public partial class FormGuppeneditor : Form
    {

        public FormGuppeneditor()
        {
            InitializeComponent();
            FormSplash.setProgress(50);
            FindMyGroupMemberships();
            FormSplash.setProgress(90);
            LoadAllUser();
            FormSplash.setProgress(95);
            FindMyGroups();
            toolStripStatusLabelVersion.Text = Application.ProductVersion;
        }

        private string _localSearchRoot;

        private List<simpleADObject> GroupMember = new List<simpleADObject>();
        private Hashtable ManagedByMe = new Hashtable();
        private Hashtable Domains = new Hashtable();
        private Hashtable DisplayToNameMap = new Hashtable();
        private List<simpleADObject> Users = new List<simpleADObject>();
        private List<simpleADObject> Groups = new List<simpleADObject>();
        private String[] currentDomains;
        private List<simpleADObject> currentGroups = new List<simpleADObject>();

        private string GetSamAccountName()
        {
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            String tmp = user.Name;
            tmp = tmp.Substring(tmp.LastIndexOf('\\')+1);
            return tmp;
        }

        private PrincipalContext[] GetPrincipalContexts()
        {
            if (Domains.Count == 0)
            {
                Domains.Add("dom1.de", new PrincipalContext(ContextType.Domain, "dom1.de", "DC=dom1,DC=de"));
                Domains.Add("dom2.de", new PrincipalContext(ContextType.Domain, "dom2.de", "DC=dom2,DC=de"));
                
            }

            return Domains.Values.Cast<PrincipalContext>().ToArray<PrincipalContext>();
        }

        private string getLocalSearchRoot()
        {
            if (_localSearchRoot == null)
            {
                DirectoryEntry rootDSE = new DirectoryEntry("LDAP://RootDSE");
                _localSearchRoot = rootDSE.Properties["defaultNamingContext"].Value.ToString();
            }
            return _localSearchRoot;
        }

        private void LoadAllUser()
        {
            try
            {
                
                foreach (PrincipalContext AD in GetPrincipalContexts())
                {
                    UserPrincipal u = new UserPrincipal(AD);
                    PrincipalSearcher search = new PrincipalSearcher(u);
                    foreach (UserPrincipal result in search.FindAll())
                    {
                        if (result.Enabled == true && result.EmailAddress != null && result.DisplayName != null)
                        {
                            simpleADObject user = new simpleADObject(result.DisplayName, result.DistinguishedName, result.EmailAddress, AD.Container);
                            Users.Add(user);
                        }
                    }
                    search.Dispose();
                }
            }

            catch (Exception e)
            {
                showError(e);
            }
        }

        private string FindMyDN()
        {
            try
            {
                foreach (PrincipalContext AD in GetPrincipalContexts())
                {
                    UserPrincipal u = new UserPrincipal(AD);
                    u.SamAccountName = GetSamAccountName();
                    PrincipalSearcher search = new PrincipalSearcher(u);
                    
                    Principal user = search.FindOne();
                    if (user != null)
                    {
                        search.Dispose();
                        return user.DistinguishedName;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                showError(e);
                return null;
            }
        }

        private string FindMySid()
        {
            try
            {
                foreach (PrincipalContext AD in GetPrincipalContexts())
                {
                    UserPrincipal u = new UserPrincipal(AD);
                    u.SamAccountName = GetSamAccountName();
                    PrincipalSearcher search = new PrincipalSearcher(u);

                    Principal user = search.FindOne();
                    if (user != null)
                    {
                        search.Dispose();
                        return user.Sid.ToString();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void FindMyGroupMemberships()
        {
            DirectorySearcher search = GetLocalDirectorySearcher();
            foreach (IdentityReference group in WindowsIdentity.GetCurrent().Groups)
            {
                search.Filter = "(&(objectClass=group)(objectSid=" + group.Value + "))";
                SearchResult gr = search.FindOne();
                if (gr != null)
                {
                    ManagedByMe.Add(group.Value, gr.Properties["distinguishedName"][0].ToString());
                }
            }
            foreach (DirectorySearcher dc in GetRemoteDirectorySearchers())
            {
                foreach (IdentityReference group in WindowsIdentity.GetCurrent().Groups)
                {
                    search.Filter = "(&(objectClass=foreignSecurityPrincipal)(cn=" + group.Value + "))";
                    SearchResult gr = search.FindOne();
                    if (gr != null)
                    {
                        for (int i = 0; i < gr.Properties["memberOf"].Count; i++)
                        {
                            ManagedByMe.Add(group.Value, gr.Properties["distinguishedName"][i].ToString());
                        }
                    }
                }
            }
        }


        private void FindMyGroups()
        {
            try
            {
                string dn = FindMyDN();
                string sid = FindMySid();
                if (dn == null) return;
                if (sid == null) return;
                string localFilter = "";
                if (ManagedByMe.Count > 0)
                {
                    localFilter = "(|(managedBy=" + dn + ")";
                    foreach (String s in ManagedByMe.Values)
                    {
                        localFilter += "(managedBy=" + s + ")";
                    }
                    localFilter += ")";
                }
                else
                {
                    localFilter = "(managedBy=" + dn + ")";
                }
                DirectorySearcher search = GetLocalDirectorySearcher();
                search.Filter = "(&(objectClass=group)" + localFilter + ")";
                foreach (SearchResult result in search.FindAll())
                {
                    string name = result.Properties["name"][0].ToString();
                    string groupdn = result.Properties["distinguishedName"][0].ToString();
                    simpleADObject group = new simpleADObject(name, groupdn, "", getLocalSearchRoot());
                    Groups.Add(group);
                }

                

                foreach (DirectorySearcher remoteSearch in GetRemoteDirectorySearchers())
                {
                    string domain = remoteSearch.SearchRoot.Path.ToString().Replace("LDAP://", "");
                    string remoteFilter = "(&(objectClass=group)(|(member=CN=" + sid + ",CN=ForeignSecurityPrincipals," + domain + ")";
                    foreach (IdentityReference myGroups in WindowsIdentity.GetCurrent().Groups)
                    {
                        remoteFilter += "(member=CN=" + myGroups.Value + ",CN=ForeignSecurityPrincipals," + domain + ")";
                    }
                    remoteFilter += "))";
                    remoteSearch.Filter = remoteFilter;
                    localFilter = "(|";
                    foreach (SearchResult result in remoteSearch.FindAll())
                    {
                        string groupdn = result.Properties["distinguishedName"][0].ToString();
                        localFilter += "(managedBy=" + groupdn + ")";
                    }
                    localFilter += ")";
                    remoteSearch.Filter = localFilter;
                    foreach (SearchResult result in remoteSearch.FindAll())
                    {
                        string name = result.Properties["name"][0].ToString();
                        string groupdn = result.Properties["distinguishedName"][0].ToString();
                        simpleADObject group = new simpleADObject(name, groupdn, "", domain);
                        Groups.Add(group);
                    }
                }

                comboBoxGruppe.Items.Clear();
                Groups.Sort(delegate(simpleADObject c1, simpleADObject c2) { return c1.DisplayName.CompareTo(c2.DisplayName); });
                string prevGroup = "";
                List<string> groupDomains = new List<string>();
                DisplayToNameMap.Clear();

                if (Groups.Count > 0)
                {
                    var maxLength = Groups.Max(a => a.DisplayName.Length);
                    string format = "{0, -" + maxLength + "} ({1})";
                    foreach (simpleADObject group in Groups.ToArray())
                    {
                        if (group.DisplayName.ToLower() == prevGroup.ToLower())
                        {
                            groupDomains.Add(group.Domain);
                        }
                        else
                        {
                            if (prevGroup != "" && groupDomains.Count > 0)
                            {
                                string text = String.Format(format, prevGroup, String.Join(", ", groupDomains.ToArray()));
                                comboBoxGruppe.Items.Add(text);
                                DisplayToNameMap.Add(text, prevGroup);
                            }
                            prevGroup = group.DisplayName;
                            groupDomains = new List<string>();
                            groupDomains.Add(group.Domain);
                        }
                    }
                    if (prevGroup != "" && groupDomains.Count > 0)
                    {
                        string text = String.Format(format, prevGroup, String.Join(", ", groupDomains.ToArray()));
                        comboBoxGruppe.Items.Add(text);
                        DisplayToNameMap.Add(text, prevGroup);
                    }
                    comboBoxGruppe.Items.Add("bitte wählen");
                    comboBoxGruppe.SelectedIndexChanged -= this.comboBoxGruppe_SelectedIndexChanged;
                    comboBoxGruppe.SelectedIndex = comboBoxGruppe.Items.IndexOf("bitte wählen");
                    comboBoxGruppe.SelectedIndexChanged += this.comboBoxGruppe_SelectedIndexChanged;
                }
            }
            catch (Exception e)
            {
                showError(e);
            }
        }

        private SearchResult GetUserForDN(string dn)
        {
            try
            {
                DirectorySearcher search = GetDirectorySearcherForDN(dn);
                search.Filter = "(distinguishedName=" + dn + ")";
                return search.FindOne();
            }
            catch (Exception e)
            {
                showError(e);
                return null;
            }
        }

        private void LoadGroup(string groupname)
        {
            try
            {
                currentGroups = Groups.Where(a => a.DisplayName.ToLower() == groupname.ToLower()).ToList();
                currentDomains = currentGroups.Select(a => a.Container).ToArray();
                clearMemberList();
                foreach (string Container in currentDomains)
                {
                    string dn = currentGroups.Where(a => a.Container == Container).Select(a => a.DistinguishedName).ToArray()[0];
                    DirectorySearcher search = GetDirectorySearcherForContainer(Container);

                    search.Filter = "(distinguishedName=" + dn + ")";
                    SearchResult result = search.FindOne();
                    for (int i = 0; i < result.Properties["member"].Count; i++)
                    {
                        addUserToMemberList(result.Properties["member"][i].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                showError(e);
            }
        }

        private DirectorySearcher GetLocalDirectorySearcher()
        {
            DirectoryEntry ldapConnection = new DirectoryEntry();
            ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
            return new DirectorySearcher(ldapConnection);
        }

        private DirectorySearcher[] GetRemoteDirectorySearchers()
        {
            List<DirectorySearcher> remote = new List<DirectorySearcher>();
            string root = getLocalSearchRoot();
            foreach (PrincipalContext PC in Domains.Values.Cast<PrincipalContext>().Where<PrincipalContext>(a => a.Container.ToString() != root).ToArray<PrincipalContext>())
            {
                DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://" + PC.Container);
                ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
                remote.Add(new DirectorySearcher(ldapConnection));
            }
            return remote.ToArray();
        }

        private DirectorySearcher[] GetDirectorySearchers()
        {
            List<DirectorySearcher> searchers = new List<DirectorySearcher>();
            foreach (PrincipalContext PC in GetPrincipalContexts())
            {
                DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://" + PC.Container);
                ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
                searchers.Add(new DirectorySearcher(ldapConnection));
            }
            return searchers.ToArray();
        }

        private DirectorySearcher[] GetDirectorySearchers(String[] Container)
        {
            List<DirectorySearcher> searchers = new List<DirectorySearcher>();
            foreach (PrincipalContext PC in Domains.Values.Cast<PrincipalContext>().Where<PrincipalContext>(a => Container.Contains(a.Container.ToString())).ToArray<PrincipalContext>())
            {
                DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://" + PC.Container);
                ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
                searchers.Add(new DirectorySearcher(ldapConnection));
            }
            return searchers.ToArray();
        }

        private DirectorySearcher GetDirectorySearcherForContainer(string Container)
        {
            string[] Containers = { Container };
            DirectorySearcher[] tmp = GetDirectorySearchers(Containers);
            if (tmp.Length > 0)
            {
                return tmp[0];
            }
            return null;
        }

        private DirectorySearcher GetDirectorySearcherForDN(String dnOrSid)
        {
            foreach (PrincipalContext PC in GetPrincipalContexts())
            {
                if (dnOrSid.Contains(PC.Container))
                {
                    DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://"+PC.Container);
                    ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
                    return new DirectorySearcher(ldapConnection);
                }
            }
            return null;
        }

        private SearchResult GetUserForSid(string sid)
        {
            foreach (PrincipalContext PC in GetPrincipalContexts())
            {
                DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://" + PC.Container);
                ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
                DirectorySearcher search = new DirectorySearcher(ldapConnection);
                search.Filter = "(ObjectSid=" + sid + ")";
                SearchResult sr = search.FindOne();
                if (sr != null && ! sr.Path.Contains(",CN=ForeignSecurityPrincipals,"))
                {
                    return sr;
                }
            }
            return null;
        }

        private string GetSidForUser(SearchResult user)
        {
            if (user == null) return null;
            if (user.Properties.Contains("ObjectSid"))
            {
                var sidInBytes = (byte[])user.Properties["ObjectSid"][0];
                var sid = new SecurityIdentifier(sidInBytes, 0);
                return sid.ToString();
            }
            else
            {
                return null;
            }

        }

        private void comboBoxGruppe_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UseWaitCursor = true;
            if (comboBoxGruppe.SelectedItem.ToString() != "bitte wählen")
            {
                comboBoxGruppe.Items.Remove("bitte wählen");
                LoadGroup(DisplayToNameMap[comboBoxGruppe.SelectedItem.ToString()].ToString());
                comboBoxMember.AutoCompleteCustomSource.Clear();
                comboBoxMember.AutoCompleteCustomSource.AddRange(Users.Where(a => currentDomains.Contains(a.Container)).Select(a => a.DisplayNameWithDomain).ToArray());
                if (currentDomains.Length < Domains.Count)
                {
                    string comma = "";
                    string missing = "";
                    foreach (string key in Domains.Keys)
                    {
                        if (!currentDomains.Contains(((PrincipalContext)Domains[key]).Container))
                        {
                            missing += comma + key;
                        }
                        comma = ", ";
                    }
                    toolStripStatusLabelDomain.Text = "Nicht unterstützte Domains: " + missing;
                }
                else
                {
                    toolStripStatusLabelDomain.Text = "";
                }
                groupBoxMember.Enabled = true;
                buttonSave.Enabled = false;
                if (comboBoxMember.Text.Length == 0)
                {
                    buttonAdd.Enabled = false;
                }
                if (Users.Find(a => a.DisplayNameWithDomain == comboBoxMember.Text) == null)
                {
                    buttonAdd.Enabled = false;
                }
            }
            this.UseWaitCursor = false;
        }

        private void addUserToMemberList(string distinguishedName)
        {
            string displayName;
            string container = null;
            string domain = null;
            SearchResult user;
            if (distinguishedName.Contains("CN=ForeignSecurityPrincipals"))
            {
                string sid = distinguishedName.Split(',')[0].Split('=')[1];
                user = GetUserForSid(sid);
            }
            else
            {
                user = GetUserForDN(distinguishedName);
            }
            if (user == null) return;
            
            foreach(PrincipalContext pc in Domains.Values) {
                if (user.Path.Contains(pc.Container))
                {
                    domain = pc.Container.Replace(",DC=", ".").Replace("DC=", "");
                }
                if (distinguishedName.Contains(pc.Container))
                {
                    container = pc.Container;
                }
            }
            if (domain == null)
            {
                throw new NotImplementedException("Unbekannte Domain für " + user.Path);
            }
            try
            {
                displayName = user.Properties["displayName"][0].ToString();
            }
            catch
            {
                ResultPropertyValueCollection t = user.Properties["name"];
                if (t.Count >= 1)
                {
                    displayName = user.Properties["name"][0].ToString();
                }
                else
                {
                    MessageBox.Show("Beim Bearbeiten dieser Gruppe kann es zu Fehlern kommen, da nicht unterstüzte Objekte in dieser Gruppe enthalten sind. Wenden Sie sich ggf. an die Hotline.");
                    return;
                }
            }
            if (GroupMember.Find(a => a.DistinguishedName == distinguishedName) == null)
            {
                simpleADObject obj;
                ListViewItem lvi = new ListViewItem(displayName);
                if (user.Properties.Contains("mail"))
                {
                    lvi.SubItems.Add(user.Properties["mail"][0].ToString().ToLowerInvariant());
                    obj = new simpleADObject(displayName, distinguishedName, user.Properties["mail"][0].ToString().ToLowerInvariant(), container);
                }
                else
                {
                    lvi.SubItems.Add("keine Email Adresse vorhanden");
                    obj = new simpleADObject(displayName, distinguishedName, null, container);
                }
                lvi.SubItems.Add(domain);
                listViewMember.Items.Add(lvi);
                GroupMember.Add(obj);
            }
            else
            {
                MessageBox.Show("Benutzer ist bereits Mitglied", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void clearMemberList()
        {
            listViewMember.Items.Clear();
            GroupMember.Clear();
        }
        
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            simpleADObject[] users = Users.Where(a => a.DisplayNameWithDomain.ToLowerInvariant() == comboBoxMember.Text.ToLowerInvariant()).ToArray();
            if (users.Length == 1) {
                simpleADObject user = users[0];
                if (currentGroups.Where(a => a.Container == user.Container).ToArray().Length > 0)
                {
                    addUserToMemberList(user.DistinguishedName);
                    buttonSave.Enabled = true;
                }
            }
            else
            {
                if (users.Length > 1)
                {
                    MessageBox.Show("Benutzer nicht eindeutig gefunden", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Benutzer nicht gefunden", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }

        private void comboBoxMember_KeyDown(object sender, KeyEventArgs e)
        {
            if (Users.Find(a => a.DisplayNameWithDomain.ToLower() == comboBoxMember.Text.ToLower()) != null)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (buttonAdd.Enabled)
                    {
                        buttonAdd_Click(this, null);
                    }
                    else
                    {
                        buttonAdd.Enabled = true;
                    }
                }
            }
            else
            {
                buttonAdd.Enabled = false;
            }
        }


        private void buttonRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewMember.CheckedItems)
            {
                GroupMember = GroupMember.Where(a => !(a.DisplayName == item.Text && a.Domain == item.SubItems[2].Text)).ToList();
                item.Remove();
            }
            buttonSave.Enabled = true;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (simpleADObject cGroup in currentGroups)
                {
                    DirectorySearcher search = GetDirectorySearcherForContainer(cGroup.Container);
                    if (search == null)
                    {
                        continue;
                    }

                    search.Filter = "(distinguishedName=" + cGroup.DistinguishedName + ")";
                    SearchResult result = search.FindOne();
                    DirectoryEntry group = result.GetDirectoryEntry();
                    group.Properties["member"].Clear();
                    foreach (simpleADObject obj in GroupMember.Where(a => a.Container == cGroup.Container))
                    {
                        group.Properties["member"].Add(obj.DistinguishedName);
                    }
                    group.CommitChanges();
                    buttonSave.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                showError(ex);
            }
        }

        private void showError(Exception e)
        {
            MessageBox.Show(e.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private void listViewMember_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listViewMember.CheckedItems.Count > 0)
            {
                buttonRemove.Enabled = true;
            }
            else
            {
                buttonRemove.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Activate();
            timer1.Enabled = false;
        }

        private void FormGuppeneditor_Shown(object sender, EventArgs e)
        {
            FormSplash.setProgress(100);
            timer1.Enabled = true;
            if (Groups.Count == 0)
            {
                MessageBox.Show("Sie sind bei keiner Gruppe als verwaltungsberechtigt eingetragen.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }

    public class simpleADObject
    {
        private string _DisplayName;
        private string _DistinguishedName;
        private string _EmailAddress;
        private string _Domain;
        private string _Container;

        public string DisplayName { 
            get {
            return _DisplayName;
            }
            set {
                _DisplayName = value;
            }
        }

        public string DistinguishedName
        {
            get
            {
                return _DistinguishedName;
            }
            set
            {
                _DistinguishedName = value;
            }
        }

        public string EmailAddress
        {
            get
            {
                return _EmailAddress;
            }
            set
            {
                _EmailAddress = value;
            }
        }

        public string Domain
        {
            get
            {
                return _Domain;
            }
            
        }

        public string DisplayNameWithDomain
        {
            get
            {
                return _DisplayName + " [" + _Domain + "]";
            }
        }

        public string Container
        {
            get
            {
                return _Container;
            }
            set
            {
                _Container = value;
                _Domain = value.Replace(",DC=", ".").Replace("DC=", "");
            }
        }

        public simpleADObject(string DisplayName, string DistinguishedName, string EmailAddress, string Container) {
            this.DisplayName = DisplayName;
            this.DistinguishedName = DistinguishedName;
            this.EmailAddress = EmailAddress;
            this.Container = Container;
        }
    }
}
