﻿//------------------------------------------------------------------------------
// <copyright file="OpenOnGitLab.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using GitLab.VisualStudio.Services;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using GitLab.VisualStudio.Shared;

namespace GitLab.VisualStudio
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenOnGitLab
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6cd6c755-f5a3-4944-a799-44b346edd2ea");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

      
     
        private static DTE2 _dte;

        internal DTE2 DTE
        {
            get
            {
                if (_dte == null)
                {
                    _dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
                }

                return _dte;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOnGitLab"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenOnGitLab(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                AddCommand(commandService, PackageIds.CommandId_OpenMaster,VSPackage.OpenOnGitLab_OpenOnGitLab_OpenMaster);
                AddCommand(commandService, PackageIds.CommandId_OpenBranch, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenBranch);
                AddCommand(commandService, PackageIds.CommandId_OpenRevision, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenRevision);
                AddCommand(commandService, PackageIds.CommandId_OpenRevisionFull, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenRevisionFull);
                AddCommand(commandService, PackageIds.CommandId_Commits, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenCommits);
                AddCommand(commandService, PackageIds.CommandId_Blame, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenBlame);
            }
        }

        private void AddCommand(OleMenuCommandService commandService, int item,string text)
        {
            var menuCommandID = new CommandID(PackageGuids.guidOpenOnGitLabPackageCmdSet, (int)item);
            var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID,text);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            try
            {
                // TODO:is should avoid create GitAnalysis every call?
                using (var git = new GitAnalysis(GetActiveFilePath()))
                {
                    if (!git.IsDiscoveredGitRepository)
                    {
                        command.Enabled = false;
                        return;
                    }

                    var type = ToGitLabUrlType(command.CommandID.ID);
                    var targetPath = git.GetGitLabTargetPath(type);
                    if (type == GitLabUrlType.CurrentBranch && targetPath == "master")
                    {
                        command.Visible = false;
                    }
                    else
                    {
                       command.Text = git.GetGitLabTargetDescription(type);
                        command.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var exstr = ex.ToString();
                Debug.Write(exstr);
                //  command.Text = "error:" + ex.GetType().Name;
                command.Enabled = false;
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenOnGitLab Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new OpenOnGitLab(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string title = "OpenOnGitLab";

            // Show a message box to prove we were here

            var command = (MenuCommand)sender;
            try
            {
                using (var git = new GitAnalysis(GetActiveFilePath()))
                {
                    if (!git.IsDiscoveredGitRepository)
                    {
                        return;
                    }
                    var selectionLineRange = GetSelectionLineRange();
                    var type = ToGitLabUrlType(command.CommandID.ID);
                    var gitLabUrl = git.BuildGitLabUrl(type, selectionLineRange);
                    System.Diagnostics.Process.Start(gitLabUrl); // open browser
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
        this.ServiceProvider,
        ex.Message,
        title,
        OLEMSGICON.OLEMSGICON_INFO,
        OLEMSGBUTTON.OLEMSGBUTTON_OK,
        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            }
        }

          string GetActiveFilePath()
        {
            // sometimes, DTE.ActiveDocument.Path is ToLower but GitHub can't open lower path.
            // fix proper-casing | http://stackoverflow.com/questions/325931/getting-actual-file-name-with-proper-casing-on-windows-with-net
            var path = GetExactPathName(DTE.ActiveDocument.Path + DTE.ActiveDocument.Name);
            return path;
        }

       internal static string GetExactPathName(string pathName)
        {
            if (!(File.Exists(pathName) || Directory.Exists(pathName)))
                return pathName;

            var di = new DirectoryInfo(pathName);

            if (di.Parent != null)
            {
                return Path.Combine(
                    GetExactPathName(di.Parent.FullName),
                    di.Parent.GetFileSystemInfos(di.Name)[0].Name);
            }
            else
            {
                return di.Name.ToUpper();
            }
        }

        Tuple<int, int> GetSelectionLineRange()
        {
            var selection = DTE.ActiveDocument.Selection as TextSelection;
            if (selection == null)
            {
                return null;
            }
            return Tuple.Create(selection.TopPoint.Line, selection.BottomPoint.Line);
        }

        static GitLabUrlType ToGitLabUrlType(int commandId)
        {
            if (commandId == PackageIds.CommandId_OpenMaster) return GitLabUrlType.Master;
            if (commandId == PackageIds.CommandId_OpenBranch) return GitLabUrlType.CurrentBranch;
            if (commandId == PackageIds.CommandId_OpenRevision) return GitLabUrlType.CurrentRevision;
            if (commandId == PackageIds.CommandId_Blame) return GitLabUrlType.Blame;
            if (commandId == PackageIds.CommandId_Commits) return GitLabUrlType.Commits;
            if (commandId == PackageIds.CommandId_OpenRevisionFull)
                return GitLabUrlType.CurrentRevisionFull;
            return GitLabUrlType.CurrentRevisionFull;
        }
    }
}
