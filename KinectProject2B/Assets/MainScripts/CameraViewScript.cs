/* Main Cameraの視点切替についての処理
 * 画面の強制スクロールの再現をする処理(カメラのz座標を時間経過で増加させる)
 * 特定のポーズをとると、視点切替
 */

using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Linq;
using UnityEngine.SceneManagement;

public class CameraViewScript : MonoBehaviour
{
    public BodySourceManager _BodyManager;
    public Body[] bodies; // Bodyの情報を格納する
    public float z_pos; // 元の視点の時のz座標の初期値(今回は、x, y座標を動かす必要は無い)
    public float z2_pos; // 右腕を挙げて視点切替をしたときのz座標の初期値(今回は、x, y座標を動かす必要は無い)
    public float z_add; // カメラのz軸方向の移動の際の増加量の初期値(スクロール速度の代わり)
    public MainScript ms_Camera; // MainScriptのフィールド参照用変数

    // Use this for initialization
    void Start()
    {
        bodies = _BodyManager.GetData();
        z_pos = -2;
        z2_pos = 3;
        z_add = 0.001f;
        ms_Camera = GetComponent<MainScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_BodyManager == null)
        { // Kinectに、プレーヤーの骨格情報が伝わっていない
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

        // 獲得単位数でスクロールの速度を変える
        if (ms_Camera.Score <= 45)
        {
            z_add = 0.001f;
        }
        else if (46 <= ms_Camera.Score && ms_Camera.Score <= 90)
        {
            z_add = 0.005f;
        }
        else if (91 <= ms_Camera.Score && ms_Camera.Score <= 110)
        {
            z_add = 0.01f;
        }
        else
        {
            z_add = 0.05f;
        }
        
        // カメラの位置を移動させる
        z_pos += z_add;
        z2_pos += z_add;

        // カメラの視点切替
        // 腕を挙げているかどうかの判定に使用
        if (a_body_data != null)
        {
            Windows.Kinect.Joint HandRight = a_body_data.Joints[JointType.HandRight];
            Windows.Kinect.Joint HandLeft = a_body_data.Joints[JointType.HandLeft];
            Windows.Kinect.Joint Head = a_body_data.Joints[JointType.Head];
            bool RaiseHand = HandRight.Position.Y > Head.Position.Y || HandLeft.Position.Y > Head.Position.Y;

            if (HandRight.Position.Y > Head.Position.Y)
            { // 右腕または左腕、あるいは両腕を頭よりも高く上げたとき
                // カメラのアングルを一時的に変える
                transform.position = new Vector3(3, 1, z2_pos); // x, z座標は変化し、y座標は変化無し

                transform.rotation = Quaternion.Euler(0, -120, 0);
            }
            else if (RaiseHand == false)
            { // 腕が挙げられていない、または腕の位置が戻ったとき
                // カメラのアングルを戻す
                transform.position = new Vector3(2.5f, 1, z_pos);
                transform.rotation = Quaternion.Euler(0, -20, 0);
            }
            else
            {
                // 何もしない
            }
        }
    }
}