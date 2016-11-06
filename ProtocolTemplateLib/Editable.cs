﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace ProtocolTemplateLib
{
    public abstract class Editable : ITemplatePart
    {
        public bool EnableOtherField { get; set; }

        public string Id { get; set; }

        public abstract void SaveXml(XmlWriter writer);

        protected abstract string PartOfCreateTableScript();

        public string GetPartOfCreateTableScript()
        {
            string result = PartOfCreateTableScript();
            if (EnableOtherField)
            {
                result += ", " + Id + "_other nvarchar(255) NOT NULL";
            }
            return result;
        }

        public static Editable GetFromXml(XmlNode node)
        {
            Editable result;
            switch (node.Name)
            {
                case NodeNameComboBox:
                    result = new ComboboxEditable();
                    break;
                case NodeNameTextBox:
                    result = new TextBoxEditable();
                    break;
                default:
                    throw new XmlException(String.Format("Wrong node name for Editable. Not found '{0}' type.", node.Name));
            }
            result.LoadFromXml(node);
            return result;
        }

        public abstract object GetValueFromControl();
        public abstract void SetValueToControl(Object value);
        public abstract Control GetEditControl();
        public abstract string PrintToProtocol(object value);
        public abstract string PrintToSaveQuery(object value);

        protected static void LocateControlStandart(Control control)
        {
            control.VerticalAlignment = VerticalAlignment.Top;
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.Margin = new Thickness(5);
        }

        protected void SaveOtherEnabled(XmlWriter writer)
        {
            writer.WriteAttributeString(AttributeNameOtherEnabled, EnableOtherField.ToString());
        }

        protected void LoadOtherEnabled(XmlNode node)
        {
            XmlUtils.AssertAttributeNotNull(node, AttributeNameOtherEnabled);
            try
            {
                EnableOtherField = Boolean.Parse(node.Attributes[AttributeNameOtherEnabled].Value);
            }
            catch (FormatException ex)
            {
                throw new XmlException(string.Format("Error loading template. Attribute '{0}' is not boolean in node '{1}'", AttributeNameOtherEnabled, node.Name), ex);
            }
        }
        internal abstract string GetTypeName();

        protected abstract void LoadFromXml(XmlNode node);

        protected const string AttributeNameOtherEnabled = "other";
        protected const string NodeNameComboBox = "combobox";
        protected const string NodeNameTextBox = "textbox";

    }

    public class ComboboxEditable : Editable
    {
        private const string NodeNameVariant = "variant";
        private const string AttributeNameValue = "value";

        public List<String> Variants { get; set; }

        public override Control GetEditControl()
        {
            ComboBox control = new ComboBox();
            LocateControlStandart(control);
            control.ItemsSource = Variants;
             
            control.IsEditable = EnableOtherField;
            lastComboBox = control;
            return control;
        }

        public override string PrintToProtocol(object value)
        {
            throw new NotImplementedException();
        }

        public override void SaveXml(XmlWriter writer)
        {
            writer.WriteStartElement(NodeNameComboBox);
            SaveOtherEnabled(writer);
            foreach (var item in Variants)
            {
                writer.WriteStartAttribute(NodeNameVariant);
                writer.WriteAttributeString(AttributeNameValue, item);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        protected override string PartOfCreateTableScript()
        {
            return Id + " int";
        }

        protected override void LoadFromXml(XmlNode node)
        {
            XmlUtils.AssertNodeName(node, NodeNameComboBox);
            LoadOtherEnabled(node);
            List<string> variants = new List<string>();
            foreach (XmlNode item in node.ChildNodes)
            {
                XmlUtils.AssertNodeName(item, NodeNameVariant, true);
                XmlUtils.AssertAttributeNotNull(item, AttributeNameValue);
                variants.Add(item.Attributes[AttributeNameValue].Value);
            }
            Variants = variants;
        }

        public override string PrintToSaveQuery(object value)
        {
            if (EnableOtherField)
            {
                string realValue = (String)value;
                int index = Variants.IndexOf(realValue);
                if (index < 0)
                {
                    return "NULL, " + realValue;
                }
                else
                {
                    return index + ", NULL";
                }
            }
            else
            {
                return ((int)value).ToString();
            }
        }

        public override object GetValueFromControl()
        {
            if (EnableOtherField)
            {
                return lastComboBox.SelectedValue;
            }
            else
            {
                return lastComboBox.SelectedIndex;
            }
        }

        public override void SetValueToControl(Object value)
        {
            if (EnableOtherField) {
                lastComboBox.SelectedValue = (String)value;
            }
            else
            {
                lastComboBox.SelectedIndex = (int)value;
            }
        }

        internal override string GetTypeName()
        {
            return "выпадающий список";
        }

        private ComboBox lastComboBox;
    }
    public class TextBoxEditable : Editable
    {
        public override Control GetEditControl()
        {
            lastControl = new TextBox();
            LocateControlStandart(lastControl);
            return lastControl;
        }

        public override string PrintToProtocol(object value)
        {
            throw new NotImplementedException();
        }

        public override void SaveXml(XmlWriter writer)
        {
            writer.WriteStartElement(NodeNameTextBox);
            writer.WriteEndElement();
        }

        protected override void LoadFromXml(XmlNode node)
        {
            // No properties
            XmlUtils.AssertNodeName(node, NodeNameTextBox);
        }

        protected override string PartOfCreateTableScript()
        {
            return Id + " nvarchar(1024)";
        }

        public override string PrintToSaveQuery(object value)
        {
            return (string)value;
        }

        public override object GetValueFromControl()
        {
            return lastControl.Text;
        }

        public override void SetValueToControl(Object value)
        {
            lastControl.Text = (String)value;
        }

        internal override string GetTypeName()
        {
            return "текстовоe поле";
        }

        private TextBox lastControl;
    }
}
