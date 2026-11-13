## 概要
ASP.NET Core Web APIとPostgreSQLを使用した在庫管理システムです。
商品・倉庫/ロケーション別の在庫を、入庫・出庫・移動・棚卸の各処理を通じて管理します。
開発したアプリケーションはDockerコンテナ化し、AWS(ECS Fargate)上へのデプロイ・検証まで行います。

## 開発目的
C#の既存業務経験をベースに、ASP.NET CoreによるWeb API開発、PostgreSQLを利用したデータベース連携、Dockerによるコンテナ化、およびAWSを使ったクラウド環境の構築・検証を習得する。

## 開発期間
2026年11月(19営業日)

## 開発環境
- OS: Windows 11
- IDE: Visual Studio 2026
- Language: C#
- Framework: ASP.NET Core / .NET 10
- Database: PostgreSQL 18
- ORM: Entity Framework Core 10
- API Documentation: Swagger / OpenAPI
- Test: xUnit
- Container: Docker
- Source Control: Git / GitHub
- CI/CD: GitHub Actions(予定)
- Cloud: AWS(VPC、IAM、ECR、ECS Fargate、ALB、RDS、CloudWatch)

## 開発予定機能
- [x] DBマイグレーション(EF Core)
- [x] 商品マスタ(登録・参照・更新・削除)
- [x] 倉庫・ロケーションマスタ
- [x] 入庫処理
- [x] 出庫処理
- [x] 在庫引当・マイナス在庫防止
- [ ] 排他制御・トランザクション管理
- [ ] 在庫照会(検索・集計)
- [ ] 在庫移動
- [ ] 棚卸
- [ ] 在庫下限アラート
- [ ] 入出庫履歴(検索)
- [ ] CSV出力
- [ ] 管理画面(UI)
- [ ] 入力チェック・例外処理

認証・権限(Cognito等)は対象外です。必要になった場合に追加を検討します。

## 開発スケジュール
| 期間 | 内容 |
|---|---|
| 11/2 〜 11/6 | 調査・設計、共通基盤、商品マスタ |
| 11/9 〜 11/11 | 倉庫・ロケーションマスタ、Docker化、AWS環境構築、初回デプロイ |
| 11/12 〜 11/16 | 入庫・出庫、在庫引当、排他制御 |
| 11/17 〜 11/20 | 在庫照会、在庫移動、棚卸、アラート、入出庫履歴 |
| 11/24 〜 11/25 | CSV出力、管理画面 |
| 11/26 〜 11/30 | テスト・検証、報告書作成、最終デプロイ |

## ディレクトリ構成
```text
AideaInventoryMng
├── AideaInventoryMng.slnx
├── AideaInventoryMng        アプリ本体
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Controllers
│   ├── Data
│   │   └── ApplicationDbContext.cs
│   └── Migrations
├── docs                     設計書・手順書
└── README.md
```

## ドキュメント
設計書・手順書は `docs` フォルダにMarkdownで管理しています。

| ファイル | 内容 | 状況 |
|---|---|---|
| [01_system-overview.md](docs/01_system-overview.md) | システム概要・構成設計書 | 初版 |
| [02_database-design.md](docs/02_database-design.md) | データベース設計書 | 初版 |
| [03_api-design.md](docs/03_api-design.md) | API設計書 | 初版 |
| [04_processing-design.md](docs/04_processing-design.md) | 処理設計書 | 初版 |
| 05_docker-setup.md | Docker環境構築手順書 | 作成予定 |
| 06_aws-setup.md | AWS環境構築手順書 | 作成予定 |
| 07_test-spec-and-result.md | テスト仕様書・結果 | 作成予定 |
| 08_verification-report.md | 動作検証報告書 | 作成予定 |
| 09_issues-and-solutions.md | 問題点・解決方法の一覧 | 作成予定 |
| 10_final-report.md | 最終学習報告書 | 作成予定 |



\## 日報

11/2(月)

Entity Framework のマイグレーションについて

https://qiita.com/yutotakakura/items/31ab539321502deacd88


11/4(水)

データベース設計書（02_database-design.md）の作成

Entity Framework Coreの設定

ProductsControllerの作成

Product Entityの作成


11/5(木)
Product のDTO作成完了

Warehouse のEntity、DTO、Controllerの作成

設計書の7.2・7.3修正完了

在庫数量を stocks テーブルで管理する方針を確認


11/6(金)

データベース設計に基づき、Entity Framework Core（EF Core）の初期マイグレーションを再作成し、PostgreSQLへの適用を完了

これにより、在庫管理システムの基盤となる7テーブルをデータベース上に構築できた。

11/9（月）

入庫のためのReceiptsController のPost作成

上記のPostに関係するDTOを３つ作成

上記の内容をSwaggerにて確認テスト実行


11/10(火)

出庫のためのIssuesController のPost作成

上記のPostに関係するDTOを３つ作成

上記の内容をSwaggerにて確認テスト実行



11/11(水)

在庫の引当・引当解除のためのStocksController のPost作成

上記のPostに関係するDTOを2つ作成

上記の内容をSwaggerにて確認テスト実行


11/12(木)
Postgresの排他制御について
https://zenn.dev/tazzae999jp/articles/c48597a1b4348a

「人間が操作する時間」は楽観ロック、「DB更新の瞬間」だけ悲観ロック
https://qiita.com/NagaokaKenichi/items/73040df85b7bd4e9ecfc

在庫の棚卸のためのStocktakesController のPost作成
Postgresでの楽観ロック・悲観ロック・排他制御を確認
上記のPostに関係するDTOを2つ作成
上記の内容をSwaggerにて確認テスト実行

11/13(金)
在庫の棚卸のためのStocktakesController のGetとPut作成
上記のPutに関係するDTOを2つ作成
Put時の実棚数量の入力チェック、ステータスチェック、棚卸明細の存在確
上記の内容をSwaggerにて確認テスト実行


