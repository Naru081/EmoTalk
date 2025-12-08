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

    // ==================== 共通SQL処理関数(INSERT・UPDATE・DELETE) ====================
    function ExecuteUpdate($sql, $param)
    {
        try {
            $stmt = $this->pdo->prepare($sql);
            $result = $stmt->execute($param);   // SQL実行 

            // 注 $rowは、処理の場合によっては0を返す可能性がある (実行は成功したが影響行数が0(UPDATEで同じ値を書いた場合など)) その場合は次のelse内の処理が動く
            $row = $stmt->rowCount();

            if ($result && $row > 0) {
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
