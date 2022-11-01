using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo.Metadata;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XafExportUsersAndRoles.Module.BusinessObjects;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Xpo;

namespace XafExportUsersAndRoles.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class ExportRolesController : ViewController
    {
        SimpleAction RestoreBackup;
        SimpleAction Export;
        IDataLayer dl = GetBackupDal();
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public ExportRolesController()
        {
            InitializeComponent();
            Export = new SimpleAction(this, "Export Roles", "View");
            Export.Execute += Export_Execute;

            RestoreBackup = new SimpleAction(this, "Restore Roles", "View");
            RestoreBackup.Execute += RestoreBackup_Execute;
            

            //this.TargetObjectType= typeof(ApplicationUser);
            this.TargetObjectType = typeof(PermissionPolicyRole);
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        private void RestoreBackup_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            UnitOfWork unitOfWork = new UnitOfWork(dl);
            var CurrentDal = (this.ObjectSpace as XPObjectSpace).Session.DataLayer;
       

            CloneHelper cloneHelper = new CloneHelper((this.ObjectSpace as XPObjectSpace).Session);
            var Roles=   unitOfWork.Query<PermissionPolicyRole>().ToList();

            var Users = unitOfWork.Query<PermissionPolicyUser>().ToList();
            unitOfWork.Delete(Users);
            unitOfWork.PurgeDeletedObjects();

            foreach (var item in Roles)
            {
                cloneHelper.Clone(item);
            }
            this.View.ObjectSpace.CommitChanges();
            // Execute your business logic (https://docs.devexpress.com/eXpressAppFramework/112737/).
        }
        private void Export_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            File.Delete("Backup.db3");
           

            UnitOfWork targetSession = new UnitOfWork(dl);
            CloneHelper cloneHelper = new CloneHelper(targetSession);


            cloneHelper.Clone(this.View.CurrentObject);
            //var Users=this.ObjectSpace.CreateCollection(typeof(ApplicationUser));
            //foreach (object? user in Users)
            //{
            //    cloneHelper.Clone(user);
            //}
            targetSession.CommitChanges();


            // Execute your business logic (https://docs.devexpress.com/eXpressAppFramework/112737/).
        }

        private static IDataLayer GetBackupDal()
        {
            string conn = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString("Backup.db3");
            IDataLayer dl = XpoDefault.GetDataLayer(conn, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            using (Session session = new Session(dl))
            {


                session.UpdateSchema(typeof(ApplicationUser), typeof(PermissionPolicyRole));
                session.CreateObjectTypeRecords(typeof(ApplicationUser), typeof(PermissionPolicyRole));

            }

            return dl;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
    }
}
