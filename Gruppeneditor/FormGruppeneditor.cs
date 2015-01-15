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
            FindMyMemberships();
            //GetTrustDomains();
            FormSplash.setProgress(90);
            FindAllUser();
            FormSplash.setProgress(95);
            FindMyGroups();
        }

        private bool _cannotAddTrustUser;
        private string _localSearchRoot;

        private Hashtable UserDNTable = new Hashtable();
        private Hashtable GroupDNTable = new Hashtable();
        private Hashtable GroupMember = new Hashtable();
        private Hashtable ManagedByMe = new Hashtable();
        private Hashtable Domains = new Hashtable();

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

        private void FindAllUser()
        {
            try
            {
                comboBoxMember.AutoCompleteCustomSource.Clear();
                foreach (PrincipalContext AD in GetPrincipalContexts())
                {
                    UserPrincipal u = new UserPrincipal(AD);
                    PrincipalSearcher search = new PrincipalSearcher(u);
                    foreach (UserPrincipal result in search.FindAll())
                    {
                        if (result.Enabled == true && result.EmailAddress != null && result.DisplayName != null)
                        {
                            string tmp = String.Format("{0}", result.DisplayName);
                            if (!UserDNTable.ContainsKey(tmp.ToLowerInvariant()))
                            {
                                UserDNTable.Add(tmp.ToLowerInvariant(), result.DistinguishedName);
                                comboBoxMember.AutoCompleteCustomSource.Add(tmp);
                            }
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

        private void FindMyMemberships()
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
        }


        private void FindMyGroups()
        {
            try
            {
                DirectorySearcher search = GetLocalDirectorySearcher();
                string dn = FindMyDN();
                if (dn == null) return;
                string filter = "";
                if (ManagedByMe.Count > 0)
                {
                    filter = "(|(managedBy=" + dn + ")";
                    foreach (String s in ManagedByMe.Values)
                    {
                        filter += "(managedBy=" + s + ")";
                    }
                    filter += ")";
                }
                else
                {
                    filter = "(managedBy=" + dn + ")";
                }
                search.Filter = "(&(objectClass=group)" + filter + ")";
                comboBoxGruppe.Items.Clear();
                GroupDNTable.Clear();
                foreach (SearchResult result in search.FindAll())
                {
                    string name = result.Properties["name"][0].ToString();
                    comboBoxGruppe.Items.Add(name);
                    if (GroupDNTable.ContainsKey(name.ToLowerInvariant()))
                    {
                        throw new NotImplementedException();
                    }
                    GroupDNTable.Add(name.ToLowerInvariant(), result.Properties["distinguishedName"][0]);
                }
                comboBoxGruppe.Items.Add("bitte wählen");
                comboBoxGruppe.SelectedIndexChanged -= this.comboBoxGruppe_SelectedIndexChanged;
                comboBoxGruppe.SelectedIndex = comboBoxGruppe.Items.IndexOf("bitte wählen");
                comboBoxGruppe.SelectedIndexChanged += this.comboBoxGruppe_SelectedIndexChanged;
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
                DirectorySearcher search = GetDirectorySearcher(dn);
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
                DirectorySearcher search = GetLocalDirectorySearcher();
                search.Filter = "(distinguishedName=" + GroupDNTable[groupname.ToLowerInvariant()] + ")";
                clearMemberList();
                SearchResult result = search.FindOne();
                for (int i = 0; i < result.Properties["member"].Count; i++)
                {
                    addUserToMemberList(result.Properties["member"][i].ToString());
                }
                Int32 groupType = (Int32)result.Properties["groupType"][0];
                _cannotAddTrustUser = ((groupType & 0x4) == 0x4); // Local Group oder nicht
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

        private DirectorySearcher GetDirectorySearcher(String dnOrSid)
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
            if (comboBoxGruppe.SelectedItem.ToString() != "bitte wählen")
            {
                comboBoxGruppe.Items.Remove("bitte wählen");
                LoadGroup(comboBoxGruppe.SelectedItem.ToString());
                groupBoxMember.Enabled = true;
                buttonSave.Enabled = false;
                if (comboBoxMember.Text.Length == 0)
                {
                    buttonAdd.Enabled = false;
                }
                if (!UserDNTable.ContainsKey(comboBoxMember.Text.ToLowerInvariant()))
                {
                    buttonAdd.Enabled = false;
                }
            }
        }

        private void addUserToMemberList(string distinguishedName)
        {
            string displayName;
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
                try
                {
                    displayName = user.Properties["displayName"][0].ToString();
                }
                catch (Exception e)
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

            if (!GroupMember.ContainsKey(displayName.ToLowerInvariant()))
            {
                ListViewItem lvi = new ListViewItem(displayName);
                if (user.Properties.Contains("mail"))
                {
                    lvi.SubItems.Add(user.Properties["mail"][0].ToString().ToLowerInvariant());
                }
                else
                {
                    lvi.SubItems.Add("keine Email Adresse vorhanden");
                }
                listViewMember.Items.Add(lvi);
                GroupMember.Add(displayName.ToLowerInvariant(), distinguishedName);
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

        private void removeMember(string displayName)
        {
            foreach (ListViewItem item in listViewMember.Items)
            {
                if (item.Text.Equals(displayName))
                {
                    item.Remove();
                }
            }
            GroupMember.Remove(displayName.ToLowerInvariant());
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (UserDNTable.ContainsKey(comboBoxMember.Text.ToLowerInvariant()))
            {
                string userdn = UserDNTable[comboBoxMember.Text.ToLowerInvariant()].ToString();
                if (!_cannotAddTrustUser)
                {
                    if (userdn.Contains(getLocalSearchRoot()))
                    {
                        addUserToMemberList(userdn);
                        buttonSave.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Dieser Benutzer ist Teil einer anderen Domäne. " +
                        "Möchten Sie diesen Benutzer hinzufügen, muss der Typ dieser Gruppe von der IT vorher bearbeitet werden. (Stichwort: Domänen lokal)", "Hinzufügen leider nicht möglich", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    addUserToMemberList(userdn);
                    buttonSave.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Benutzer nicht gefunden", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void comboBoxMember_KeyDown(object sender, KeyEventArgs e)
        {
            if (UserDNTable.ContainsKey(comboBoxMember.Text.ToLowerInvariant()))
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
                removeMember(item.Text);
            }
            buttonSave.Enabled = true;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                DirectorySearcher search = GetLocalDirectorySearcher();
                
                search.Filter = "(distinguishedName=" + GroupDNTable[comboBoxGruppe.SelectedItem.ToString().ToLowerInvariant()] + ")";
                SearchResult result = search.FindOne();
                DirectoryEntry group = result.GetDirectoryEntry();
                group.Properties["member"].Clear();
                group.CommitChanges();

                foreach (string distinguishedName in GroupMember.Values)
                {
                    string sid = GetSidForUser(GetUserForDN(distinguishedName));
                    if (sid != null)
                    {
                        group.Invoke("Add", new Object[] { String.Format("LDAP://<SID={0}>", sid) });
                        //group.Properties["member"].Add(String.Format("LDAP://<SID={0}>", sid));
                    }
                }
                
                buttonSave.Enabled = false;
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
            if (GroupDNTable.Count == 0)
            {
                MessageBox.Show("Sie sind bei keiner Gruppe als verwaltungsberechtigt eingetragen.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

    }
}
