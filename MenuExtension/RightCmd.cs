﻿// ***********************************************************************
// Solution         : MyselfTools
// Project          : MenuExtension
// File             : RightCmd.cs
// ***********************************************************************
// <copyright>
//     Copyright © 2016 - 2017 Kolibre Credit Team. All rights reserved.
// </copyright>
// ***********************************************************************

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MenuExtension
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class RightCmd
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 0x1004;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("FEE10FE9-F5DD-4D15-B1F0-563DDC3C8D25");

        /// <summary>
        ///     VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RightCmd" /> class.
        ///     Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private RightCmd(Package package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                CommandID menuCommandID = new CommandID(CommandSet, CommandId);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static RightCmd Instance { get; private set; }

        /// <summary>
        ///     Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => package;

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new RightCmd(package);
        }

        /// <summary>
        ///     This function is the callback used to execute the command when the menu item is clicked.
        ///     See the constructor to see how the menu item is associated with this function using
        ///     OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", GetType().FullName);
            string title = "RightCmd";

            //string dic = Environment.CurrentDirectory;

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}