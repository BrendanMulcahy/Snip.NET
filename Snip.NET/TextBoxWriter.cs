using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace SnipDotNet
{
    public class TextBoxWriter : TextWriter
    {
        TextBox textBox = null;

        public TextBoxWriter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}