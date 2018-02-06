/* タイトル画面で特定のポーズをとるとできる操作の処理
 * 前回のプレーのスコアを表示する
 * ゲーム画面へ切り替える
 * or ゲームを終了する
*/

using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Text型のオブジェクトの宣言に使用

public class StartMenu : MonoBehaviour
{
    public BodySourceManager _BodyManager;
    public MainScript ms_SMenu; // MainScriptのフィールド参照用変数
    public Text previous_scoreText; // 前回プレー時のスコア表示用テキスト変数
    public int previous_score = 0; // スコア計算用変数

    // Use this for initialization
    void Start()
    {
        ms_SMenu = GetComponent<MainScript>();
        previous_scoreText.text = "";
    }

    // Update is called once per frame
    public void Update()
    {
        if (previous_scoreText != null) {
            previous_score = ms_SMenu.Score; // previous_socreに、MainScriptのScoreの値を参照、格納する
            previous_scoreText.text = "前回のプレーのスコア: " + previous_score.ToString();
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

    public Vector3 ToVector3(CameraSpacePoint point)
    { // CameraSpacePoint型からVector3型へ変換する
        return new Vector3(point.X, point.Y, point.Z);
    }

    public float length(Vector3 v)
    { // ベクトルの長さ（大きさ）を計算する
        return (float) System.Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    public float dot(Vector3 v1, Vector3 v2)
    { // 2つのベクトルの内積を計算する
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }

    public float cosine(Vector3 v1, Vector3 v2)
    { // 2つのベクトルのなす角cosθ(余弦)を計算する
        return dot(v1, v2) / (length(v1) * length(v2));
    }

    public float acosine(float cosine) {
        // 関数cosineで計算した余弦の逆関数を計算する
        return (float) System.Math.Acos(cosine);
    }

    public int checkSelection(Windows.Kinect.Body body)
    { // どのポーズを取っているかをチェックする

        if (body != null)
        { // 骨格情報が認識されているとき
            // 腕を挙げているかどうかの判定に使用
            Windows.Kinect.Joint HandRight = body.Joints[JointType.HandRight];
            Windows.Kinect.Joint HandLeft = body.Joints[JointType.HandLeft];
            Windows.Kinect.Joint Head = body.Joints[JointType.Head];
            // どちらか片方の膝が曲がっているかどうかの判定に使用
            Windows.Kinect.Joint HipRight = body.Joints[JointType.HipRight];
            Windows.Kinect.Joint HipLeft = body.Joints[JointType.HipLeft];
            Windows.Kinect.Joint KneeRight = body.Joints[JointType.KneeRight];
            Windows.Kinect.Joint KneeLeft = body.Joints[JointType.KneeLeft];
            Windows.Kinect.Joint AnkleRight = body.Joints[JointType.AnkleRight];
            Windows.Kinect.Joint AnkleLeft = body.Joints[JointType.AnkleLeft];

            Vector3 LegRight1 = ToVector3(HipRight.Position) - ToVector3(KneeRight.Position);
            Vector3 LegRight2 = ToVector3(AnkleRight.Position) - ToVector3(KneeRight.Position);
            Vector3 LegLeft1 = ToVector3(HipLeft.Position) - ToVector3(KneeLeft.Position);
            Vector3 LegLeft2 = ToVector3(AnkleLeft.Position) - ToVector3(KneeLeft.Position);

            // 両膝の角度
            float KneeRight_Angle = acosine(cosine(LegRight1, LegRight2));
            float KneeLeft_Angle = acosine(cosine(LegLeft1, LegLeft2));

            if (HandRight.Position.Y > Head.Position.Y && HandLeft.Position.Y > Head.Position.Y)
            { // 両腕を頭よりも高く上げたとき、ゲーム画面へ切り替える
                return 1;
            }
            else if ((acosine(1) < KneeRight_Angle && KneeRight_Angle < acosine(1 / 2)) ||
                (acosine(1) < KneeLeft_Angle && KneeLeft_Angle < acosine(1 / 2)))
            { // どちらか片方の膝が0～60度の間までしっかり曲がったとき、ゲームを終了する
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
            SceneManager.LoadScene("GameDisplay");
        }
        else if (checkSelection(body) == 2) {
            // ゲームを終了する
            Application.Quit();
        }
        // 他の場合(checkSelection() = 0)は、何もしない
    }
}