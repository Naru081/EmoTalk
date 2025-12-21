README 12月19日(金曜日)

◻️プロファイル新規作成関連
・CreateProfileInputPanel.cs -> 名前・性格・口調・一人称の入力
・CreateProfileModelSelect.cs -> モデル選択画面(新規作成用)
・CreateProfileWindow.cs -> 新規作成の流れを管理する親ウインドウ
//==========================================================


◻️削除関連
・DeleteConfirmPopup.cs -> 削除確認画面
・DeleteDeniedPopup.cs -> 削除不可
//==========================================================


◻️ログアウト
・LogoutPopup.cs -> ログアウト確認画面(ログイン画面に遷移するだけ)
//==========================================================


◻️メインUI制御
・MenuController.cs -> ハンバーガーメニューの制御
・TopController.cs -> Live2Dモデル画面の制御
//==========================================================


◻️音声入力
・MicPopupController.cs -> マイク入力(後からWhisper APIと接続)
//==========================================================


◻️モデルの切替管理
・ModelManager.cs -> モデルを切り替えと、その保持
・ModelSelectWindow.cs -> モデルUIの一覧(新規作成用)
・ModelChangeWindow.cs -> モデル変更UIの一覧
//==========================================================


◻️プロファイル
・ProfileManager.cs -> プロファイルのリスト管理
・ProfileController.cs -> 先頭データの動作
・ProfileListWindow.cs -> データリストの表示
・ProfileListWrapper.cs -> リストのレイアウト制御
・ProfileEditWindow.cs -> プロファイル画面
・ProfilePropertyWindow.cs -> プロパティ画面
・PropertyWindow.cs -> 上記のUI表示
//==========================================================


◻️データモデル
・ProfileData.cs -> プロファイル1件分のデータ構造
//==========================================================


APIに置き換わるところ(推定)
⭐️ProfileEditWindow.cs⭐️
・データの永続化
・プロファイル番号
・選択中のプロファイル保持
