﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Kontract.Interface;
using Kontract;
using runext.Properties;

namespace runext
{
    public partial class ExtensionSelect : Form
    {
        #region Properties

        public IExtension Extension { get; set; }

        #endregion

        public ExtensionSelect()
        {
            InitializeComponent();
            Extension = null;
        }

        private void ExtensionSelect_Load(object sender, EventArgs e)
        {
            Icon = Resources.kuriimu;

            // Load Extensions
            IEnumerable<IExtension> extensions = PluginLoader<IExtension>.LoadPlugins(Settings.Default.PluginDirectory, "ext*.dll");

            // Populate List
            int index = 0;
            imgList.TransparentColor = Color.Transparent;
            foreach (IExtension extension in extensions)
            {
                imgList.Images.Add(extension.Icon);
                imgList.Images.SetKeyName(index, extension.Name);
                TreeNode node = new TreeNode(extension.Name, index, index);
                node.Tag = extension;
                treExtensions.Nodes.Add(node);
                index++;
            }

            if (treExtensions.Nodes.Count > 0)
                treExtensions.SelectedNode = treExtensions.Nodes[0];
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Extension = (IExtension)treExtensions.SelectedNode.Tag;
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}