/* ゲームプレー時のプレーヤーをUnityちゃんモデルに反映させる処理
 * Unityちゃんモデル(プレーヤー)と他のオブジェクトとの当たり判定の処理
 * ゲームオーバーとなったときの画面切替(シーン遷移)の処理
 * スコア計算と更新、その表示の処理
 */

using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI; // Text型のオブジェクトの宣言に使用
using UnityEngine.SceneManagement;

public class MainScript : MonoBehaviour
{

    public bool IsMirror = true;
    public BodySourceManager _BodyManager;
    public GameObject _UnityChan;

    public GameObject Ref;
    public GameObject Hips;
    public GameObject LeftUpLeg;
    public GameObject LeftLeg;
    public GameObject RightUpLeg;
    public GameObject RightLeg;
    public GameObject Spine1;
    public GameObject Spine2;
    public GameObject LeftShoulder;
    public GameObject LeftArm;
    public GameObject LeftForeArm;
    public GameObject LeftHand;
    public GameObject RightShoulder;
    public GameObject RightArm;
    public GameObject RightForeArm;
    public GameObject RightHand;
    public GameObject Neck;
    public GameObject Head;

    public Text creditsText; // スコア表示用変数
    public Text startText; // Kinectのカメラへの身体全体の認識を促すテキストの表示用変数
    public int Score; // スコア計算用変数

    // 無回転状態qを生成
    Quaternion q = new Quaternion();
    public GameObject[] Objs; // ランダムにオブジェクトを生成するための配列
    // ランダムなオブジェクト生成・座標の設定にカメラの位置のz座標を使用
    public CameraViewScript cvs;
    float cvs_z;

    public bool PlayerCollision; // プレーヤーが障害物(床と単位以外の他のオブジェクト)と衝突したかどうか
    public int ObjBorder; // 新たにオブジェクトを生成する境界線

    // Use this for initialization
    void Start()
    {
        Ref = _UnityChan.transform.Find("Character1_Reference").gameObject;

        Hips = Ref.gameObject.transform.Find("Character1_Hips").gameObject;
        LeftUpLeg = Hips.transform.Find("Character1_LeftUpLeg").gameObject;
        LeftLeg = LeftUpLeg.transform.Find("Character1_LeftLeg").gameObject;
        RightUpLeg = Hips.transform.Find("Character1_RightUpLeg").gameObject;
        RightLeg = RightUpLeg.transform.Find("Character1_RightLeg").gameObject;
        Spine1 = Hips.transform.Find("Character1_Spine").
                    gameObject.transform.Find("Character1_Spine1").gameObject;
        Spine2 = Spine1.transform.Find("Character1_Spine2").gameObject;
        LeftShoulder = Spine2.transform.Find("Character1_LeftShoulder").gameObject;
        LeftArm = LeftShoulder.transform.Find("Character1_LeftArm").gameObject;
        LeftForeArm = LeftArm.transform.Find("Character1_LeftForeArm").gameObject;
        LeftHand = LeftForeArm.transform.Find("Character1_LeftHand").gameObject;
        RightShoulder = Spine2.transform.Find("Character1_RightShoulder").gameObject;
        RightArm = RightShoulder.transform.Find("Character1_RightArm").gameObject;
        RightForeArm = RightArm.transform.Find("Character1_RightForeArm").gameObject;
        RightHand = RightForeArm.transform.Find("Character1_RightHand").gameObject;
        Neck = Spine2.transform.Find("Character1_Neck").gameObject;
        Head = Neck.transform.Find("Character1_Head").gameObject;

        creditsText.text = "Credits: 0"; // 初期スコアを代入して画面に表示
        startText.text = "";
        Score = 0;
        // ランダム生成されるオブジェクトの初期化
        Objs = new GameObject[4];
        // 床
        Objs[0] = Instantiate(Resources.Load("RunningFloor", typeof(GameObject)), new Vector3(0, -5, 20), Quaternion.identity) as GameObject;
        Objs[0].transform.localScale = new Vector3(3, 10, 50);
        // 単位ｰ1
        Objs[1]= Instantiate(Resources.Load("CreditsItem_Minus", typeof(GameObject)), new Vector3(-0.5f, 0.5f, 4), Quaternion.identity) as GameObject;
        Objs[1].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        // 単位+1
        Objs[2] = Instantiate(Resources.Load("CreditsItem_Plus", typeof(GameObject)), new Vector3(1, 0.5f, 7), Quaternion.identity) as GameObject;
        Objs[2].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        // 障害物
        Objs[3] = Instantiate(Resources.Load("Obstracle", typeof(GameObject)), new Vector3(0, 2, 5), Quaternion.identity) as GameObject;
        Objs[3].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        // カメラの位置のz座標の初期化
        cvs = GetComponent<CameraViewScript>();
        cvs_z = cvs.z_pos;

        PlayerCollision = false;
        ObjBorder = 120;
    }

    // Update is called once per frame
    void Update()
    {
        if (_BodyManager == null)
        {
            Debug.Log("_BodyManager == null");
            return;
        }

        // Bodyデータを取得する
        var data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        // 最初に追跡している人を取得する
        var body = data.FirstOrDefault(b => b.IsTracked);
        if (body == null)
        {
            // 最初に追跡している人を取得できない = カメラに映っていない
            startText.text = "Kinectのカメラが身体を認識していません。\n身体全体をカメラに認識させてください！";
            return;
        }

        startText.text = ""; // 骨格情報がカメラに認識されたので、プレー可能

        // 床の傾きを取得する
        var floorPlane = _BodyManager.FloorClipPlane;
        var comp = Quaternion.FromToRotation(
            new Vector3(floorPlane.X, floorPlane.Y, floorPlane.Z), Vector3.up);

        // 関節の回転を取得する
        var joints = body.JointOrientations;

        Quaternion SpineBase;
        Quaternion SpineMid;
        Quaternion SpineShoulder;
        Quaternion ShoulderLeft;
        Quaternion ShoulderRight;
        Quaternion ElbowLeft;
        Quaternion WristLeft;
        Quaternion HandLeft;
        Quaternion ElbowRight;
        Quaternion WristRight;
        Quaternion HandRight;
        Quaternion KneeLeft;
        Quaternion AnkleLeft;
        Quaternion KneeRight;
        Quaternion AnkleRight;

        // 鏡
        if (IsMirror)
        {
            SpineBase = joints[JointType.SpineBase].Orientation.ToMirror().ToQuaternion(comp);
            SpineMid = joints[JointType.SpineMid].Orientation.ToMirror().ToQuaternion(comp);
            SpineShoulder = joints[JointType.SpineShoulder].Orientation.ToMirror().ToQuaternion(comp);
            ShoulderLeft = joints[JointType.ShoulderRight].Orientation.ToMirror().ToQuaternion(comp);
            ShoulderRight = joints[JointType.ShoulderLeft].Orientation.ToMirror().ToQuaternion(comp);
            ElbowLeft = joints[JointType.ElbowRight].Orientation.ToMirror().ToQuaternion(comp);
            WristLeft = joints[JointType.WristRight].Orientation.ToMirror().ToQuaternion(comp);
            HandLeft = joints[JointType.HandRight].Orientation.ToMirror().ToQuaternion(comp);
            ElbowRight = joints[JointType.ElbowLeft].Orientation.ToMirror().ToQuaternion(comp);
            WristRight = joints[JointType.WristLeft].Orientation.ToMirror().ToQuaternion(comp);
            HandRight = joints[JointType.HandLeft].Orientation.ToMirror().ToQuaternion(comp);
            KneeLeft = joints[JointType.KneeRight].Orientation.ToMirror().ToQuaternion(comp);
            AnkleLeft = joints[JointType.AnkleRight].Orientation.ToMirror().ToQuaternion(comp);
            KneeRight = joints[JointType.KneeLeft].Orientation.ToMirror().ToQuaternion(comp);
            AnkleRight = joints[JointType.AnkleLeft].Orientation.ToMirror().ToQuaternion(comp);
        }
        // そのまま
        else
        {
            SpineBase = joints[JointType.SpineBase].Orientation.ToQuaternion(comp);
            SpineMid = joints[JointType.SpineMid].Orientation.ToQuaternion(comp);
            SpineShoulder = joints[JointType.SpineShoulder].Orientation.ToQuaternion(comp);
            ShoulderLeft = joints[JointType.ShoulderLeft].Orientation.ToQuaternion(comp);
            ShoulderRight = joints[JointType.ShoulderRight].Orientation.ToQuaternion(comp);
            ElbowLeft = joints[JointType.ElbowLeft].Orientation.ToQuaternion(comp);
            WristLeft = joints[JointType.WristLeft].Orientation.ToQuaternion(comp);
            HandLeft = joints[JointType.HandLeft].Orientation.ToQuaternion(comp);
            ElbowRight = joints[JointType.ElbowRight].Orientation.ToQuaternion(comp);
            WristRight = joints[JointType.WristRight].Orientation.ToQuaternion(comp);
            HandRight = joints[JointType.HandRight].Orientation.ToQuaternion(comp);
            KneeLeft = joints[JointType.KneeLeft].Orientation.ToQuaternion(comp);
            AnkleLeft = joints[JointType.AnkleLeft].Orientation.ToQuaternion(comp);
            KneeRight = joints[JointType.KneeRight].Orientation.ToQuaternion(comp);
            AnkleRight = joints[JointType.AnkleRight].Orientation.ToQuaternion(comp);
        }

        // 関節の回転を計算する
        var q = transform.rotation;
        transform.rotation = Quaternion.identity;

        var comp2 = Quaternion.AngleAxis(90, new Vector3(0, 1, 0)) *
                    Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));

        Spine1.transform.rotation = SpineMid * comp2;

        RightArm.transform.rotation = ElbowRight * comp2;
        RightForeArm.transform.rotation = WristRight * comp2;
        RightHand.transform.rotation = HandRight * comp2;

        LeftArm.transform.rotation = ElbowLeft * comp2;
        LeftForeArm.transform.rotation = WristLeft * comp2;
        LeftHand.transform.rotation = HandLeft * comp2;

        RightUpLeg.transform.rotation = KneeRight * comp2;
        RightLeg.transform.rotation = AnkleRight * comp2;

        LeftUpLeg.transform.rotation = KneeLeft * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));
        LeftLeg.transform.rotation = AnkleLeft * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));

        // モデルの回転を設定する
        transform.rotation = q;

        // モデルの位置を移動する
        var pos = body.Joints[JointType.SpineMid].Position;
        Ref.transform.position = new Vector3(pos.X, pos.Y, 0);
        /* サンプルコードとしているKinectAvatar.csでは
         Ref.transform.position = new Vector3(-pos.X, pos.Y, -pos.Z);
         と書いているが、ここでは少々改変。
         ・プレーヤーが動いた左右どちらかと同じ向きにUnityちゃんが動くように、x座標はpos.Xに。
         ・プレーヤーの動きによるZ軸方向の移動をなくすため、z座標は0に。
         ・y座標は変更無し。
        */

        // 「代入された側の変数に格納」されたカメラの位置のz座標の更新
        cvs = GetComponent<CameraViewScript>();
        cvs_z = GetComponent<CameraViewScript>().z_pos;

        if (transform.position.z > ObjBorder) { // ランダムにオブジェクト生成
            CreateObject();
            ObjBorder += 120; // 新たにオブジェクトをランダム生成するタイミングを遅らせる
        }

        SceneLoad(body);
    }

    //*****************************************************************************************************

    public void CreateObject()
    { // 新たなオブジェクトをランダムに生成する

        GameObject Obj; // 配列Objsから選ばれたオブジェクトを格納する変数
        if (Random.Range(0, Objs.Length) == 0) { // 床
            // z座標、いじる
            Obj = Instantiate(Objs[0], new Vector3(0, -5, cvs_z + Random.Range(50, 71)), q);
            Obj.transform.localScale = new Vector3(3, 10, Random.Range(10, 51));
        }
        else if (Random.Range(0, Objs.Length) == 1)
        { // 単位ｰ1
            // z座標、いじる
            Obj = Instantiate(Objs[1], new Vector3(Random.Range(-0.5f, 1), 0.5f, cvs_z), q);
            Obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        else if (Random.Range(0, Objs.Length) == 2)
        { // 単位+1
            // z座標、いじる
            Obj = Instantiate(Objs[2], new Vector3(Random.Range(-0.5f, 1), 0.5f, cvs_z), q);
            Obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        else
        { // 障害物
            // z座標、いじる
            Obj = Instantiate(Objs[3], new Vector3(Random.Range(-0.2f, 0.8f), Random.Range(0.4f, 2), cvs_z), q);
            Obj.transform.localScale = new Vector3(Random.Range(0.3f, 0.8f), Random.Range(0.3f, 0.8f), Random.Range(0.3f, 0.8f));
        }
    }

    public bool GameOverJudge()
    { // ゲームオーバーかどうかを判定する
        if (transform.position.y < 100) { // 穴に落ちたとき(y座標の値は任意)
            PlayerCollision = true;
        }
        
        if(PlayerCollision == true) { // 障害物と衝突したとき
            return true;
        }
        return false;
    }

    public void OnCollisionEnter(Collision collision)
        // 引数collisionにはぶつかった相手側のオブジェクトの情報が格納される
    { // プレーヤーの3Dモデルが障害物(床と単位を除く他のオブジェクト)と衝突したら
        if (collision.gameObject.name == "CreditsItem_Plus") { // 単位+1のSphere
            // スコアの更新・表示処理
            Score += 1;
            creditsText.text = "Credits: " + Score.ToString();
        } else if (collision.gameObject.name == "CreditsItem_Minus") { // 単位ｰ1のSphere
            // スコアの更新・表示処理
            Score -= -1;
            creditsText.text = "Credits: " + Score.ToString();
        } else if (collision.gameObject.name == "Obstracle") { // 障害物のCube
            PlayerCollision = true;
        }
    }

    public void SceneLoad(Windows.Kinect.Body body)
    { // プレー終了時(ゲームオーバーと判定されたとき)に、リザルト画面へ切り替える
        if (GameOverJudge() == true)
        {
            SceneManager.LoadScene("ResultDisplay");
        }
    }
}