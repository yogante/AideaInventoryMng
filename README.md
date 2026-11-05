\# AideaInventoryMng



\## 概要

ASP.NET Core Web APIとPostgreSQLを使用した在庫管理システムです。 商品・倉庫/ロケーション別の在庫を、入庫・出庫・移動・棚卸の各処理を通じて管理します。 開発したアプリケーションはDockerコンテナ化し、AWS(ECS Fargate)上へのデプロイ・検証まで行います。

\## 開発環境

\- OS: Windows 11

\- IDE: Visual Studio 2026

\- Language: C#

\- Framework: ASP.NET Core / .NET 10

\- Database: PostgreSQL 18

\- ORM: Entity Framework Core 10

\- API Documentation: Swagger / OpenAPI

\-  Test: xUnit

\- Container: Docker

\- Source Control: Git / GitHub

\- CI/CD: GitHub Actions(予定)

\- Cloud: AWS(VPC、IAM、ECR、ECS Fargate、ALB、RDS、CloudWatch)



\## 開発目的

C#の既存業務経験をベースに、ASP.NET CoreによるWeb API開発、PostgreSQLを利用したデータベース連携、Dockerによるコンテナ化、およびAWSを使ったクラウド環境の構築・検証を習得する。



\## 開発予定機能

\- \[x] DBマイグレーション(EF Core)

\- \[ ] 商品マスタ(登録・参照・更新・削除)

\- \[ ] 倉庫・ロケーションマスタ

\- \[ ] 入庫処理

\- \[ ] 出庫処理

\- \[ ] 在庫引当・マイナス在庫防止

\- \[ ] 排他制御・トランザクション管理

\- \[ ] 在庫照会(検索・集計)

\- \[ ] 在庫移動

\- \[ ] 棚卸

\- \[ ] 在庫下限アラート

\- \[ ] 入出庫履歴(検索)

\- \[ ] CSV出力

\- \[ ] 管理画面(UI)

\- \[ ] 入力チェック・例外処理



\## ドキュメント

設計書・手順書は `docs` フォルダにMarkdownで管理しています。



| ファイル | 内容 | 状況 |

|---|---|---|

| \[01\_system-overview.md](docs/01\_system-overview.md) | システム概要・構成設計書 | 初版 |

| 02\_database-design.md | データベース設計書 | 作成予定 |

| 03\_api-design.md | API設計書 | 作成予定 |

| 04\_docker-setup.md | Docker環境構築手順書 | 作成予定 |

| 05\_aws-setup.md | AWS環境構築手順書 | 作成予定 |

| 06\_test-spec-and-result.md | テスト仕様書・結果 | 作成予定 |

| 07\_verification-report.md | 動作検証報告書 | 作成予定 |

| 08\_issues-and-solutions.md | 問題点・解決方法の一覧 | 作成予定 |

| 09\_final-report.md | 最終学習報告書 | 作成予定 |



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

