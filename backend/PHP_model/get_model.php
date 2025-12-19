<?php
// DBから全modelデータの取得を行うPHP

require_once __DIR__ . '/../common_function.php';

// modelデータ取得処理-DBmodel.php
$model = $DBmodel->ModelData();

// 全モデルデータを返す
echo json_encode($model, JSON_UNESCAPED_UNICODE);
?>