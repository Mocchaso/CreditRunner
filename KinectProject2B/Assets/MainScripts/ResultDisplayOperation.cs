/* リザルト画面で特定のポーズをとるとできる操作の処理
 * タイトル画面へ切り替える
 * or ゲームをを終了する
*/

using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Text型のオブジェクトの宣言に使用

public class ResultDisplayOperation : MonoBehaviour
{
    public BodySourceManager _BodyManager;
    public MainScript ms_Result; // MainScriptのフィールド参照用変数
    public Text ResultText; // ゲームの結果表示用変数

    // Use this for initialization
    void Start()
    {
        ms_Result = GetComponent<MainScript>();
        ResultText.text = ""; // 獲得単位数、評価、教授からの一言を同時に表示するための変数
    }

    // Update is called once per frame
    void Update()
    {
        // スコアによって結果表示を変更
        if (ResultText != null && ms_Result != null)
        {
            if (ms_Result.Score <= 45)
            {
                ResultText.text = "獲得単位数: " + ms_Result.Score.ToString() + 
                    "\n評価: まだ1年生レベルですよーん\n教授からの一言→「ちゃんとやりましょう。」";
            }
            else if (46 <= ms_Result.Score && ms_Result.Score <= 90)
            {
                ResultText.text = "獲得単位数: " + ms_Result.Score.ToString() + 
                    "\n評価: あなたは2年生レベルですね。\n教授からの一言→「もっと頑張りましょう。」";
            }
            else if (91 <= ms_Result.Score && ms_Result.Score <= 110)
            {
                ResultText.text = "獲得単位数: " + ms_Result.Score.ToString() + 
                    "\n評価: おっ、3年生レベルですよ！\n教授からの一言→「ま、こんな感じですかねー。」";
            }
            else if (111 <= ms_Result.Score && ms_Result.Score <= 124)
            {
                ResultText.text = "獲得単位数: " + ms_Result.Score.ToString() + 
                    "\n評価: 素晴らしい、4年生レベルですっ！！\n教授からの一言→「とても良い感じですね～！！」";
            }
            else
            {
                ResultText.text = "獲得単位数: " + ms_Result.Score.ToString() + 
                    "\n評価: めっちゃ頑張りましたね！！\n教授からの一言→「でも、本当はそんなに単位取れないYO！」";
            }
        }

        if (_BodyManager == null)
        {
            Debug.Log("_BodyManager == null");
            return;
        }

        // Bodyデータを取得する
        var BodyData = _BodyManager.GetData();
        if (BodyData == null)
        {
            return;
        }

        // 最初に追跡している人を取得する
        var a_body_data = BodyData.FirstOrDefault(b => b.IsTracked);
        if (BodyData == null)
        {
            return;
        }

        SceneLoad(a_body_data);
    }

    public int checkSelection(Windows.Kinect.Body body)
    { // どのポーズを取っているかをチェックする
        // 最初に追跡している人を取得する

        if (body != null)
        { // 骨格情報が認識されているとき
            Windows.Kinect.Joint HandRight = body.Joints[JointType.HandRight];
            Windows.Kinect.Joint HandLeft = body.Joints[JointType.HandLeft];
            Windows.Kinect.Joint Head = body.Joints[JointType.Head];

            if (HandRight.Position.Y > Head.Position.Y)
            { // 右腕を頭よりも高く上げたとき、タイトル画面へ切り替える
                return 1;
            }
            else if (HandLeft.Position.Y > Head.Position.Y)
            { // 左腕を頭よりも高く上げたとき、ゲームを終了する ※ここの条件式は、あとで変える！！
                return 2;
            }
            return 0;
        }
        return 0;
    }

    public void SceneLoad(Windows.Kinect.Body body)
    { // ゲーム画面のSceneへ遷移する
        if (checkSelection(body) == 1)
        {
            SceneManager.LoadScene("TitleDisplay");
        }
        else if (checkSelection(body) == 2)
        {
            // ゲームを終了する
            Application.Quit();
        }
        // 他の場合(checkSelection() = 0)は、何もしない
    }
}