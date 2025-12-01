<?php
// DBから全modelデータの取得を行うPHP

require_once '../dbconnect.php';
require_once '../DBPHP/DBmodel.php';

$DBmodel = new DBmodel($pdo);

// modelデータ取得処理-DBmodel.php
$model = $DBmodel->ModelData();

// 全モデルデータを返す
echo json_encode($model, JSON_UNESCAPED_UNICODE);
?>