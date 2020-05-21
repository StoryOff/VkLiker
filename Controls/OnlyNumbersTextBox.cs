using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;

namespace VkLikerMVVM
{
    class OnlyNumbersTextBox : TextBox
    {
        readonly Regex _regex = new Regex(@"\d");
        protected override async void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                if (!_regex.IsMatch(e.Text))
                    e.Handled = true;
                base.OnPreviewTextInput(e);
            });

            int textLength = Text.Length-1;
            for (int i = 0; i < textLength; i++)
            {
                if (Text.FirstOrDefault() == '0')
                {
                    if (SelectionStart > 1)
                    {
                        Text = Text.Remove(0, 1);
                        SelectionStart = Text.Length;
                    }

                    else
                    {
                        Text = Text.Remove(0, 1);
                        SelectionStart = 0;
                    }
                }
                else return;
            }
        }

        public OnlyNumbersTextBox()
        {
            MaxLength = 4;
        }
    }
}