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
using DevExpress.Persistent.BaseImpl;
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
using DevExpress.Xpo.Helpers;
using DevExpress.XtraRichEdit;

namespace XafExportUsersAndRoles.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    
    public partial class ExportRolesController : ViewController
    {
        PopupWindowShowAction BackupRolesAction;
        PopupWindowShowAction RestoreRolesAction;
        SimpleAction RestoreBackup;
        SimpleAction Export;
        IObjectSpace osBackup;
        public string BackupDatabasePath { get; set; }
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public ExportRolesController()
        {
            InitializeComponent();
            //Export = new SimpleAction(this, "Export Roles", "View");
            //Export.Execute += Export_Execute;

            //RestoreBackup = new SimpleAction(this, "Restore Roles", "View");
            //RestoreBackup.Execute += RestoreBackup_Execute;

            RestoreRolesAction = new PopupWindowShowAction(this, "Retore Roles Action", "View");
            RestoreRolesAction.Execute += RestoreRolesAction_Execute;
            RestoreRolesAction.CustomizePopupWindowParams += RestoreRolesAction_CustomizePopupWindowParams;
            RestoreRolesAction.TargetObjectType = typeof(Backup);


            BackupRolesAction = new PopupWindowShowAction(this, "Backup Roles Action", "View");
            BackupRolesAction.Execute += BackupRolesAction_Execute;
            BackupRolesAction.CustomizePopupWindowParams += BackupRolesAction_CustomizePopupWindowParams;
            BackupRolesAction.TargetObjectType = typeof(PermissionPolicyRole);


            //this.TargetObjectType= typeof(ApplicationUser);
            //this.TargetObjectType = typeof(PermissionPolicyRole);
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        private void BackupRolesAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            osBackup.CommitChanges();
        }
        string GetFileName()
        {
            return Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".db3");
        }
        private void BackupRolesAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {

            var FileName = GetFileName();
            IDataLayer dl = GetBackupDal(FileName);
            using (UnitOfWork targetSession = new UnitOfWork(dl))
            {
                CloneHelper cloneHelper = new CloneHelper(targetSession);


                foreach (object? selectedObject in this.View.SelectedObjects)
                {
                    cloneHelper.Clone(selectedObject);
                }
                targetSession.CommitChanges();
                //Delete users that come with the roles
                var Users = targetSession.Query<PermissionPolicyUser>().ToList();
                targetSession.Delete(Users);
                targetSession.PurgeDeletedObjects();
                targetSession.CommitChanges();
            }
            if (dl.Connection != null)
                if (dl.Connection.State != System.Data.ConnectionState.Closed)
                    dl.Connection.Close();


            osBackup = this.Application.CreateObjectSpace(typeof(Backup));
            var Backup = osBackup.CreateObject<Backup>();
            Backup.File = new FileData(Backup.Session);
            Backup.File.LoadFromStream("Backup.db3", new MemoryStream(File.ReadAllBytes(FileName)));
            e.View = this.Application.CreateDetailView(osBackup, Backup);

        }
        private void RestoreRolesAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
          
            var selectedSourceViewObjects = e.PopupWindowViewSelectedObjects;


            UnitOfWork unitOfWork = GetRestoreUnitOfWork();

            var Roles = unitOfWork.Query<PermissionPolicyRole>().ToList();
            CloneHelper cloneHelper = new CloneHelper((this.ObjectSpace as XPObjectSpace).Session);
            foreach (RoleBackup selectedSourceViewObject in selectedSourceViewObjects)
            {
                var RoleToRestore= unitOfWork.Query<PermissionPolicyRole>().FirstOrDefault(r => r.Name == selectedSourceViewObject.RoleName);
                cloneHelper.Clone(RoleToRestore);


            }
            this.View.ObjectSpace.CommitChanges();
            // Execute your business logic (https://docs.devexpress.com/eXpressAppFramework/112723/).
        }
        private void RestoreRolesAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            NonPersistentObjectSpace objectSpace = (NonPersistentObjectSpace)Application.CreateObjectSpace(typeof(RoleBackup));
            objectSpace.ObjectsGetting += ObjectSpace_ObjectsGetting;
            e.View = Application.CreateListView(objectSpace, typeof(RoleBackup), true);

        }
        private void ObjectSpace_ObjectsGetting(object sender, ObjectsGettingEventArgs e)
        {
            UnitOfWork unitOfWork = GetRestoreUnitOfWork();

            var Roles = unitOfWork.Query<PermissionPolicyRole>().ToList();
            List<RoleBackup> Backups = new List<RoleBackup>();
            foreach (var item in Roles)
            {
                Backups.Add(new RoleBackup() { RoleName = item.Name });
            }
            e.Objects = Backups;

        }

        private UnitOfWork GetRestoreUnitOfWork()
        {
            var CurrentBackup = this.View.CurrentObject as Backup;
            string FilePath = this.GetFileName();
            CurrentBackup.File.SaveToStream(new FileStream(FilePath, FileMode.OpenOrCreate));
            UnitOfWork unitOfWork = new UnitOfWork(GetBackupDal(FilePath));
            return unitOfWork;
        }

        private void RestoreBackup_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //UnitOfWork unitOfWork = new UnitOfWork(dl);
            //var CurrentDal = (this.ObjectSpace as XPObjectSpace).Session.DataLayer;
       

            //CloneHelper cloneHelper = new CloneHelper((this.ObjectSpace as XPObjectSpace).Session);
            //var Roles=   unitOfWork.Query<PermissionPolicyRole>().ToList();

            //var Users = unitOfWork.Query<PermissionPolicyUser>().ToList();
            //unitOfWork.Delete(Users);
            //unitOfWork.PurgeDeletedObjects();

            //foreach (var item in Roles)
            //{
            //    cloneHelper.Clone(item);
            //}
            //this.View.ObjectSpace.CommitChanges();
            // Execute your business logic (https://docs.devexpress.com/eXpressAppFramework/112737/).
        }
        private void Export_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            //File.Delete("Backup.db3");
           

            //UnitOfWork targetSession = new UnitOfWork(dl);
            //CloneHelper cloneHelper = new CloneHelper(targetSession);


            //cloneHelper.Clone(this.View.CurrentObject);
            ////var Users=this.ObjectSpace.CreateCollection(typeof(ApplicationUser));
            ////foreach (object? user in Users)
            ////{
            ////    cloneHelper.Clone(user);
            ////}
            //targetSession.CommitChanges();


            // Execute your business logic (https://docs.devexpress.com/eXpressAppFramework/112737/).
        }

        private static IDataLayer GetBackupDal(string Path)
        {
            string conn = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString(Path);
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
