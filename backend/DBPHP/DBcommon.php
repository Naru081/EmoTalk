<?PHP
class DBcommon
{
    private $pdo;

    public function __construct($pdo)
    {
        $this->pdo = $pdo;
    }

    // ==================== 共通SQL処理関数(SELECT) ==================== 
    function ExecuteSelect($sql, $param)
    {
        try {
            $stmt = $this->pdo->prepare($sql);
            $stmt->execute($param);   // SQL実行 
            $alldata = $stmt->fetchAll(PDO::FETCH_ASSOC); // 実行した結果(メッセージ情報)を取得 PDO::FETCH_ASSOCで連想配列として取得(余計な情報を省く)

            return $alldata;    // 取得した全データを返す
        } catch (PDOException $e) {
            // 例外エラーが発生した場合はfalseを返す
            return false;
        }
    }

    // ==================== 共通SQL処理関数(引数なしSELECT) ==================== 
    function ExecuteSelectNoParam($sql)
    {
        try {
            $stmt = $this->pdo->prepare($sql);
            $stmt->execute();   // SQL実行 
            $alldata = $stmt->fetchAll(PDO::FETCH_ASSOC); // 実行した結果(メッセージ情報)を取得 PDO::FETCH_ASSOCで連想配列として取得(余計な情報を省く)

            return $alldata;    // 取得した全データを返す
        } catch (PDOException $e) {
            // 例外エラーが発生した場合はfalseを返す
            return false;
        }
    }

    // ==================== 共通SQL処理関数(INSERT・UPDATE・DELETE) ====================
    function ExecuteUpdate($sql, $param)
    {
        try {
            $stmt = $this->pdo->prepare($sql);
            $result = $stmt->execute($param);   // SQL実行 

            if ($result) {
                return [
                    "success" => true,
                    "message" => "成功しました"
                ];
            } else {
                return [
                    "success" => false,
                    "message" => "失敗しました"
                ];
            }
        } catch (PDOException $e) {
            // 例外エラーが発生した場合はfalseを返す
            return [
                "success" => false,
                "message" => $e->getMessage()
            ];
        }
    }
    // ==================================================================================

}
