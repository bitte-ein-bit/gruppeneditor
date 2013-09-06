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
using System.Security.Principal;
using System.Diagnostics;

namespace Gruppeneditor
{
    public partial class FormGuppeneditor : Form
    {
        public FormGuppeneditor()
        {
            InitializeComponent();
            FormSplash.setProgress(5);
            FindMyMemberships();
            FormSplash.setProgress(90);
            FindAllUser();
            FormSplash.setProgress(95);
            FindMyGroups();
        }

        private Hashtable UserDNTable = new Hashtable();
        private Hashtable GroupDNTable = new Hashtable();
        private Hashtable GroupMember = new Hashtable();
        private Hashtable ManagedByMe = new Hashtable();

        private System.Security.Principal.WindowsIdentity GetCurrentUser()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent();
        }

        private string GetSamAccountName()
        {
            String tmp = GetCurrentUser().Name;
            // TODO
            //tmp = "TEST\\LArm";
            return tmp.Substring(tmp.LastIndexOf('\\'));
        }

        private PrincipalContext GetPrincipalContext()
        {
            // TODO
            //return new PrincipalContext(ContextType.Domain, "test.local", "TEST\\LArm", "abc123!");
            return new PrincipalContext(ContextType.Domain);
        }

        private void FindAllUser()
        {
            try
            {
                PrincipalContext AD = GetPrincipalContext();
                UserPrincipal u = new UserPrincipal(AD);
                PrincipalSearcher search = new PrincipalSearcher(u);
                comboBoxMember.AutoCompleteCustomSource.Clear();

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

            catch (Exception e)
            {
                showError(e);
            }
        }

        private string FindMyDN()
        {
            try
            {
                PrincipalContext AD = GetPrincipalContext();
                UserPrincipal u = new UserPrincipal(AD);
                u.SamAccountName = GetSamAccountName();
                PrincipalSearcher search = new PrincipalSearcher(u);

                Principal user = search.FindOne();

                search.Dispose();
                return user.DistinguishedName;
            }
            catch (Exception e)
            {
                showError(e);
                return null;
            }
        }

        private void FindMyMemberships()
        {
            DirectorySearcher search = GetDirectorySearcher();
            foreach (IdentityReference group in GetCurrentUser().Groups) {
                search.Filter = "(&(objectClass=group)(objectSid=" + group.Value + "))";
                SearchResult gr = search.FindOne();
                if (gr != null)
                {
                    Console.WriteLine("Group: " + group.Value);
                    Console.WriteLine("Group: " + gr.Properties["distinguishedName"][0]);
                    ManagedByMe.Add(group.Value, gr.Properties["distinguishedName"][0].ToString());
                }
            }
        }


        private void FindMyGroups()
        {
            try
            {
                DirectorySearcher search = GetDirectorySearcher();
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
                DirectorySearcher search = GetDirectorySearcher();
                search.Filter = "(distinguishedName=" + dn + ")";
                return search.FindOne();
            }
            catch (Exception e)
            {
                showError(e);
                return null;
            }
        }

        private void GetGroupMember(string groupname)
        {
            try
            {
                DirectorySearcher search = GetDirectorySearcher();
                search.Filter = "(distinguishedName=" + GroupDNTable[groupname.ToLowerInvariant()] + ")";
                clearMemberList();
                SearchResult result = search.FindOne();
                for (int i = 0; i < result.Properties["member"].Count; i++)
                {
                    addUserToMemberList(result.Properties["member"][i].ToString());
                }

            }
            catch (Exception e)
            {
                showError(e);
            }
        }

        private DirectorySearcher GetDirectorySearcher()
        {
            //DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://test.local");
            DirectoryEntry ldapConnection = new DirectoryEntry();
            ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
            // TODO
            //ldapConnection.Username = "LArm@test.local";
            //ldapConnection.Password = "abc123!";
            return new DirectorySearcher(ldapConnection);
        }

        private void comboBoxGruppe_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetGroupMember(comboBoxGruppe.SelectedItem.ToString());
            groupBoxMember.Enabled = true;
        }

        private void addUserToMemberList(string distinguishedName)
        {            
            SearchResult user = GetUserForDN(distinguishedName);
            if (user == null) return;
            string displayName = user.Properties["displayName"][0].ToString();
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
                addUserToMemberList(UserDNTable[comboBoxMember.Text.ToLowerInvariant()].ToString());
            }
            else
            {
                MessageBox.Show("Benutzer nicht gefunden", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            buttonSave.Enabled = true;
        }

        private void comboBoxMember_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAdd_Click(this, null);
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
                DirectorySearcher search = GetDirectorySearcher();
                search.Filter = "(distinguishedName=" + GroupDNTable[comboBoxGruppe.SelectedItem.ToString().ToLowerInvariant()] + ")";
                SearchResult result = search.FindOne();
                DirectoryEntry entry = result.GetDirectoryEntry();
                entry.Properties["member"].Clear();

                foreach (string distinguishedName in GroupMember.Values)
                {
                    entry.Properties["member"].Add(distinguishedName);
                }
                entry.CommitChanges();
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

        private void FormGroupEditor_Load(object sender, EventArgs e)
        {
            this.Activate();
            if (GroupDNTable.Count == 0)
            {
                MessageBox.Show("Sie sind bei keiner Gruppe als verwaltungsberechtigt eingetragen.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
    }
}
