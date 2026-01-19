<?PHP
require_once __DIR__ . '/../common_function.php';

// 各種APIに接続するための共通関数群

// ==================== Wisper API ====================

// Wisper APIに接続して音声データをテキストに変換する関数
function ConnectWisperAPI($tmpFile)
{
    // APIキーの設定
    $OPENAI_API_KEY = $_ENV['OPENAI_API_KEY'];
    // Wisper APIのURL
    $url = "https://api.openai.com/v1/audio/transcriptions";

    $ch = curl_init();  // cURLセッションを初期化
    curl_setopt($ch, CURLOPT_URL, $url); // APIのURLを指定
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true); // レスポンスを文字列で返すように設定

    // Wisper APIのリクエストヘッダーを設定(APIキーなど)
    curl_setopt($ch, CURLOPT_HTTPHEADER, [
        "Authorization: Bearer {$OPENAI_API_KEY}"
    ]);

    // POSTデータを設定
    $postdata = [
        // 使用するモデルを指定
        "model" => "whisper-1",
        "response_format" => "json",
        "temperature" => 0,
        // 音声ファイルをcURLで送信するための設定
        "file" => new CURLFile($tmpFile, "audio/wav", "audio.wav")
    ];

    curl_setopt($ch, CURLOPT_POST, true);   // POSTリクエストを指定
    curl_setopt($ch, CURLOPT_POSTFIELDS, $postdata);    // POSTデータを設定

    // APIリクエストを実行してレスポンスを取得
    $wisper_res = curl_exec($ch);  // cURLセッションを実行

    if ($wisper_res === false) {
        move_uploaded_file($tmpFile, __DIR__ . "/debug.wav");

        return [
            "success" => false,
            "message" => curl_error($ch)
        ];
    }
    
    curl_close($ch);        // cURLセッションを終了

    // ★追加：生レスポンス保存
    file_put_contents(__DIR__ . "/whisper_raw.log", $wisper_res);

    // JSONデコードして配列に変換
    $json = json_decode($wisper_res, true);

    if (isset($json['error'])) {
        return [
            "success" => false,
            "message" => $json['error']['message']
        ];
    }

    // データが取得できなかった場合はエラーを返す
    if (!isset($json['text'])) {
        return [
            "success" => false,
            "message" => "Wisper APIのレスポンスデータが不正です",
            "raw" => $wisper_res
        ];
    }

    // レスポンスを連想配列に変換して返す
    return [
        "success" => true,
        "text" => $json['text']
    ];
}

// ==================== ChatGPT API ====================

// ChatGPT APIに接続して返答メッセージと感情を取得する関数
function ConnectChatGPTAPI($model_name, $message_content, $prof_chara, $prof_tone, $prof_fp, $recent_messages)
{
    // APIキーの設定
    $OPENAI_API_KEY = $_ENV['OPENAI_API_KEY'];
    // ChatGPT APIのURL
    $url = "https://api.openai.com/v1/chat/completions";

    $message_config = " あなたはキャラクターAIの「{$model_name}」です。 
    AIであることやシステムの存在には一切触れてはいけません。
    常にキャラクターとしてふるまってください。

    あなたの性格は{$prof_chara} 
    あなたの口調は{$prof_tone} 
    あなたの一人称は{$prof_fp}

    【返答のルール】 

    ・返答は1〜2文 
    ・記号や絵文字は使わない 
    ・難しい言葉は使わない 
    ・説明口調やメタ発言は禁止
    ・感情を込めて自然に返答する
    ・会話を盛り上げるために適度にリアクションを入れる 
    ・会話を盛り上げるために適度に質問を返す 
    ・質問には正確に答える 
    ・必ずJSON形式のみで出力

    ユーザの発言に対して、過去の会話がある場合はそれを参考にしながら上記を厳守して返答してください。

    【感情】happy / angry / sad / natural の中から適切なあなたの感情を一つ選ぶ

    【出力形式】 

    { 
    \"response_text\": \"返答\", 
    \"response_text_hiragana\": \"ひらがな\",
    \"emotion\": \"happy | angry | sad | natural\" 
    } 
    ";

    $messages = [
        [
            "role" => "system",
            "content" => $message_config
        ]
    ];

    // 過去履歴を追加
    foreach ($recent_messages as $msg) {
        $messages[] = [
            "role" => $msg['message_sender'] == 0 ? "user" : "assistant",
            "content" => $msg['message_content']
        ];
    }

    // 今回のユーザー入力
    $messages[] = [
        "role" => "user",
        "content" => $message_content
    ];

    // APIリクエスト用のPOSTデータを組み立て
    $post_data = [
        "model" => "gpt-4o",
        "response_format" => [
            "type" => "json_object"
        ],
        "max_tokens" => 150,
        "messages" => $messages
    ];

    // cURLセッションの初期化
    $ch = curl_init();
    curl_setopt($ch, CURLOPT_URL, $url); // APIのURLを指定
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true); // レスポンスを文字列で返すように設定
    curl_setopt($ch, CURLOPT_POST, true);   // POSTリクエストを指定

    curl_setopt($ch, CURLOPT_HTTPHEADER, [  // ChatGPT APIのリクエストヘッダーを設定(APIキーなど)
        "Content-Type: application/json",
        "Authorization: Bearer {$OPENAI_API_KEY}"
    ]);

    curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($post_data, JSON_UNESCAPED_UNICODE));    // POSTデータをJSON形式で設定

    // APIリクエストを実行してレスポンスを取得
    $response_data = curl_exec($ch);  // cURLセッションを実行
    curl_close($ch);        // cURLセッションを終了

    // データが取得できなかった場合はエラーを返す
    if ($response_data === false) {
        return [
            "success" => false,
            "message" => "ChatGPT APIへの通信失敗しました"
        ];
    }

    // レスポンスデータをパースして正しい形の連想配列に変換
    $raw = json_decode($response_data, true);

    // レスポンスデータが不正な場合はエラーを返す
    if (!isset($raw['choices'][0]['message']['content'])) {
        return [
            "success" => false,
            "message" => "ChatGPT APIのレスポンスデータが不正です"
        ];
    }

    // ChatGPTの出力を取得
    $jsontext = $raw['choices'][0]['message']['content'];

    // JSONとしてパース
    $response_text = json_decode($jsontext, true);
    if ($response_text === null) {
        return [
            "success" => false,
            "message" => "ChatGPTの出力がJSONとしてパースできませんでした。",
        ];
    }

    return [
        "success" => true,
        "response_text" => $response_text['response_text'],
        "response_text_hiragana" => $response_text['response_text_hiragana'],
        "emotion" => $response_text['emotion']
    ];

}
// ==================== CoeiroInk API ====================

// CoeiroInk APIに接続してテキストを音声データに変換する関数
function ConnectCoeiroInkAPI($model_voice, $response_text_hiragana)
{
    // ===== 音声モデルのUUIDから、StyleIDを決定 =====

    // つくよみちゃん（冷静）
    if ($model_voice === "3c37646f-3881-5374-2a83-149267990abc") {
        $style_id = 0;
    }

    // アルマちゃん（ノーマル）
    if ($model_voice === "c97966b1-d80c-04f5-aba5-d30a92843b59") {
        $style_id = 10;
    }

    // 青葉くん（ノーマル）
    if ($model_voice === "d219f5ab-a50b-4d99-a26a-a9fc213e9100") {
        $style_id = 60;
    }

    // 銀河くん（叫び）
    if ($model_voice === "d312d0fb-d38d-434e-825d-cbcbfd105ad0") {
        $style_id = 74;
    }

    // 小春音あみ[あみたろ]（るんるん）
    if ($model_voice === "d93140ec-d365-11ec-8f1d-0242ac1c0002") {
        $style_id = 1564398633;
    }

    $url = "http://127.0.0.1:50032/v1/synthesis";

    $postdata = [
        "speakerUuid" => $model_voice,
        "text" => $response_text_hiragana,
        "styleId" => $style_id,
        "speedScale" => 1.0,
        "volumeScale" => 1.0,
        "prosodyDetail" => [],
        "pitchScale" => 0.0,
        "intonationScale" => 1.0,
        "prePhonemeLength" => 0.1,
        "postPhonemeLength" => 0.1,
        "outputSamplingRate" => 16000
    ];

    $ch = curl_init($url);
    curl_setopt_array($ch, [
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_POST => true,
        CURLOPT_HTTPHEADER => [
            "Content-Type: application/json"
        ],
        CURLOPT_POSTFIELDS => json_encode($postdata, JSON_UNESCAPED_UNICODE),
    ]);

    $res = curl_exec($ch);

    if ($res === false) {
        $error = curl_error($ch);
        curl_close($ch);
        file_put_contents(__DIR__ . "/coeiro_error.log", $error);
        return null;
    }
    curl_close($ch);

    // WAVかどうか判定（緩め）
    if ($res === false) {
        file_put_contents(__DIR__ . "/coeiro_error.log", "curl failed");
        return null;
    }

    // 先頭の空白・改行を除去
    $trimmed = ltrim($res);

    // RIFFチェック
    if (strncmp($trimmed, "RIFF", 4) !== 0) {
        file_put_contents(__DIR__ . "/coeiro_error.log", $trimmed);
        return null;
    }

    return $trimmed;
}

// ==================== CoeiroInk API ウォームアップ用 ====================
function WarmUpCoeiroInkAPI($model_voice)
{
    $flag_file = __DIR__ . 'coeiroink_warmup.flag';
    $warmup_interval = 60;  // 60秒(1分)に1回ウォームアップ実行

    // 60秒いない場合はウォームアップをスキップ
    if (file_exists($flag_file)) {
        $last = filemtime($flag_file);
        if (time() - $last < $warmup_interval) {
            return;
        }
    }

    // 超短文でウォームアップ
    $dummy_text = "あ";

    // 音声生成
    ConnectCoeiroInkAPI($model_voice, $dummy_text);

    // フラグファイルの更新
    touch($flag_file);
}
?>