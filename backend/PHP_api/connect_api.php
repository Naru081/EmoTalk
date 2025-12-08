<?
// 各種APIに接続するための共通関数群

// ==================== Wisper API ====================

// Wisper APIに接続して音声データをテキストに変換する関数
function ConnectWisperAPI($tmpFile, $mime, $name)
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
        "model" => "whisper-1",

        // 音声ファイルをcURLで送信するための設定
        "file" => new CURLFile($tmpFile, $mime, $name)
    ];

    curl_setopt($ch, CURLOPT_POST, true);   // POSTリクエストを指定
    curl_setopt($ch, CURLOPT_POSTFIELDS, $postdata);    // POSTデータを設定

    // APIリクエストを実行してレスポンスを取得
    $res = curl_exec($ch);  // cURLセッションを実行
    curl_close($ch);        // cURLセッションを終了

    // レスポンスを連想配列に変換して返す
    return json_decode($res, true);
}

function ConnectChatGPTAPI($message_content, $prof_chara, $prof_tone, $prof_fp)
{
    // APIキーの設定
    $OPENAI_API_KEY = $_ENV['OPENAI_API_KEY'];
    // ChatGPT APIのURL
    $url = "https://api.openai.com/v1/chat/completions";

    // リクエストデータ
    $postData = [
        "model" => "gpt-4o",   // ChatGPT APIのモデル
        "messages" => [
            ["role" => "system", "content" => 
            "あなたはLive2Dのキャラモデルと組み合わせ、ユーザとのおしゃべりをするAIです。
            会話の文字数はほどほどに抑え、message (返答文)と emotional (感情[happy, normal, 悲しい, 怒り])の中から適切な感情を返してください。"],
            ["role" => "user", "content" => $message_content]
        ]
    ];


}
?>