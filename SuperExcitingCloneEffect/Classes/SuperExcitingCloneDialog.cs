using System.Diagnostics;
using System.Windows;

namespace SuperExcitingCloneEffect.Classes
{
    internal static class SuperExcitingCloneDialog
    {
        /// <summary>
        /// 警告する
        /// </summary>
        /// <param name="message">警告内容</param>
        static public void ShowWarning(string message)
        {
            Task.Run(() =>
            {
                MessageBox.Show(
                    message,
                    PluginInfo.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            });
        }

        /// <summary>
        /// エラー表示する
        /// </summary>
        /// <param name="cont">内容</param>
        static public void ShowError(Exception ex)
        {
            MessageBoxResult result = MessageBox.Show(
                "エラーが発生しました。" +
                $"\nスクリーンショットと共に開発者sinβ（{SinBetaKunX}）までご報告お願いします。" +
                "\n（「はい(Y)」を押すとリンク先をブラウザで開きます。）" +
                "\nエラー内容" +
                "\n---" +
                "\n" + ex.Message +
                "\n---" +
                "\nエラー発生箇所:\n" +
                ex.StackTrace,
                PluginInfo.Title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Error
                );

            if (result == MessageBoxResult.Yes)
            {
                ProcessStartInfo pi = new()
                {
                    FileName = SinBetaKunX,
                    UseShellExecute = true,
                };
                Process.Start(pi);
            }
        }

        static private string SinBetaKunX => "https://x.com/sinBetaKun";
    }
}
