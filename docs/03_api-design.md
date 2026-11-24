# API設計書

| 項目 | 内容 |
|---|---|
| システム名 | AideaInventoryMng |
| 版 | 改訂版（ドラフト） |
| 作成日 | 2026-11-04 |
| 最終更新日 | 2026-11-24 |
| 関連資料 | [01_system-overview.md](01_system-overview.md)（システム概要・構成設計書）、[02_database-design.md](02_database-design.md)（データベース設計書）、[04_processing-design.md](04_processing-design.md)（処理設計書） |

※「（案）」と書かれた項目は、実装時に確認して確定する。  
※ 本書と実装に差異がある場合は、実装・テスト結果を確認したうえで本書を更新する。

---

## 1. 概要

### 1.1 目的

AideaInventoryMngが提供するWeb APIの仕様を記載する。

### 1.2 位置づけ

- 本書は、共通仕様とAPI一覧、各APIの入力項目・業務ルール・エラーをまとめた設計書である。
- 実装に合わせて、各APIの詳細を追記・更新する。
- リクエスト・レスポンスの細かい型情報は、OpenAPI（実装から自動出力）を正とし、本書にはその要点を記載する。
- トランザクション、排他制御、行ロックの詳細は `04_processing-design.md` を参照する。

### 1.3 対象

第3章のAPI一覧に記載したAPI。認証・権限は対象外とする。

---

## 2. 共通仕様

### 2.1 基本

| 項目 | 内容 |
|---|---|
| 方式 | REST（HTTP + JSON） |
| ベースパス | `/api`（バージョニングは行わない。必要になった時点で `/api/v2` を追加する） |
| 文字コード | UTF-8 |
| パスの命名 | 小文字のkebab-case、複数形（例：`stock-movements`） |
| JSONのプロパティ名 | camelCase（例：`productCode`） |
| 認証 | なし（対象外。必要になった場合に追加する） |

### 2.2 値の形式

| 種類 | 形式 | 例 |
|---|---|---|
| 日時 | ISO 8601（UTC、`Z`付き） | `2026-11-12T09:30:00Z` |
| 日付 | `yyyy-MM-dd` | `2026-11-20` |
| 数量 | 整数 | `10` |
| ID | 整数 | `1` |
| 区分値 | 英語の文字列（02_database-design.md の8章） | `RECEIPT` |

### 2.3 HTTPステータス

| ステータス | 意味 | 使う場面 |
|---|---|---|
| 200 OK | 成功 | 取得、更新、引当、確定など |
| 201 Created | 作成成功 | 登録（`Location`ヘッダーに作成した資源のURLを返す） |
| 204 No Content | 成功（本文なし） | 削除 |
| 400 Bad Request | 入力エラー | 必須項目の欠落、形式・範囲の誤り |
| 404 Not Found | 対象が存在しない | 存在しないID、削除済みの資源 |
| 409 Conflict | 業務ルール違反・競合 | コード重複、バージョン不一致、在庫不足、棚卸中、削除不可など |
| 500 Internal Server Error | サーバー内部エラー | 想定外のエラー |

### 2.4 エラー応答

ASP.NET Coreの標準形式（ProblemDetails）に、業務エラーコード `code` を加えた形式とする（案）。

業務ルール違反の例（409）

```json
{
  "code": "INSUFFICIENT_STOCK",
  "message": "在庫が不足しています。"
}
```

入力エラーの例（400）

```json
{
  "title": "入力内容に誤りがあります。",
  "status": 400,
  "code": "VALIDATION_ERROR",
  "errors": {
    "quantity": ["1以上を指定してください。"]
  }
}
```

※ 実際のエラー応答はASP.NET CoreのProblemDetailsの構成に合わせる。エラーコードとメッセージの返却方法は実装と統一する。

### 2.5 業務エラーコード（案）

| コード | ステータス | 意味 |
|---|---|---|
| VALIDATION_ERROR | 400 | 入力値の誤り |
| INVALID_ACTUAL_QUANTITY | 400 | 実棚数量が不正（負数など） |
| NOT_FOUND | 404 | 対象が存在しない（削除済みを含む） |
| DUPLICATE_CODE | 409 | コードの重複（削除済みのコードも含む） |
| VERSION_CONFLICT | 409 | バージョン不一致（他の更新と競合） |
| INSUFFICIENT_STOCK | 409 | 在庫（引当可能数）が不足 |
| INSUFFICIENT_RESERVATION | 409 | 解除する数量が引当数量を超えている |
| HAS_STOCK | 409 | 在庫が残っているため削除できない |
| STOCKTAKE_IN_PROGRESS | 409 | 棚卸が実施中の倉庫のため、入庫・出庫・引当・引当解除・移動、または対象倉庫のロケーション追加・削除などができない |
| INVALID_STOCKTAKE_STATUS | 409 | 棚卸の状態が操作に合っていない（DRAFT以外の開始、IN_PROGRESS以外の明細追加・実棚数量の入力・確定・中止） |
| DUPLICATE_STOCKTAKE_LINE | 409 | 同じ商品・同じロケーションの棚卸明細が既に存在する |
| STOCK_ALREADY_EXISTS | 409 | 対象の商品・ロケーションの在庫が既に存在する（明細の追加、または新規在庫としての確定ができない） |
| STOCKTAKE_QUANTITY_CONFLICT | 409 | 棚卸の確定時、現在の在庫数量が帳簿数量と一致しない |
| RESERVED_QUANTITY_CONFLICT | 409 | 棚卸の確定時、実棚数量が現在の引当数量を下回る |
| ACTUAL_QUANTITY_NOT_ENTERED | 409 | 実棚数量が未入力の明細が存在する |

※ `ACTUAL_QUANTITY_NOT_ENTERED` は実棚数量未入力専用のエラーコードとする。  
※ `INVALID_STATE` は、上記の個別エラーコードと意味が重複するため使用しない。  
※ エラーコードの返却条件とHTTPステータスは、実装・テスト結果と照合して確定する。

### 2.6 ページング

一覧・検索APIに共通で、次のクエリパラメータを使う。

| パラメータ | 内容 | 既定値 |
|---|---|---|
| page | ページ番号（1始まり） | 1 |
| pageSize | 1ページの件数（最大100） | 20 |

応答は次の形式とする。

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 53
}
```

### 2.7 排他制御

- 更新系のAPI（マスタの `PUT` と、棚卸の確定・中止）は、リクエストに `version` を必須とする。
- `version` が現在の値と異なる場合は、409（`VERSION_CONFLICT`）を返す。
- 更新に成功した応答には、更新後の `version` を含める。
- 棚卸の開始（`/start`）はリクエストの `version` を使わず、状態の確認と行ロックで二重実行を防ぐ。
- 棚卸明細の実棚数量の更新（`PUT /api/stocktakes/{id}/lines/{lineId}`）は `version` を使わず、棚卸ヘッダーの行ロックで排他制御する。
- 在庫更新系APIは、対象ロケーション行のロックと棚卸中チェックを行い、在庫更新まで同一トランザクションで実行する。
- 棚卸開始・明細追加・確定・中止における行ロックの順序は、`04_processing-design.md` に従う。
- 在庫テーブルの `version` は、サーバー側の在庫同時更新制御に使用する。クライアントからの更新用 `version` の送信は原則行わない。
- 行ロック、Versionによる同時実行制御、再試行の具体的な実装方法は、`04_processing-design.md` を参照する。

### 2.8 削除

- `DELETE` は論理削除とする（マスタのみ）。
- 削除済みの資源は、一覧・取得の対象外とする（取得は404）。
- 在庫が残っているものは削除できない（409、`HAS_STOCK`）。
- 商品：在庫数量または引当数量が残っている場合は削除不可。
- 倉庫：配下のロケーションに在庫数量または引当数量が残っている場合は削除不可。
- ロケーション：在庫数量または引当数量が残っている場合は削除不可。
- 倉庫・ロケーションは、対象倉庫で棚卸が実施中の場合は削除不可（409、`STOCKTAKE_IN_PROGRESS`）。
- 在庫数量と引当数量が両方0の場合は、関連する在庫行を物理削除せずに残す。
- 商品削除と入庫などの在庫更新が同時に実行された場合の競合については、`04_processing-design.md` の既知制約を参照する。

---

## 3. API一覧

| No | 機能 | メソッド | パス | 概要 | 成功時 | 実装予定 |
|---|---|---|---|---|---|---|
| 1 | 商品 | GET | /api/products | 商品の一覧・検索 | 200 | Day 4 |
| 2 |  | GET | /api/products/{id} | 商品の取得 | 200 | Day 4 |
| 3 |  | POST | /api/products | 商品の登録 | 201 | Day 4 |
| 4 |  | PUT | /api/products/{id} | 商品の更新 | 200 | Day 4 |
| 5 |  | DELETE | /api/products/{id} | 商品の削除（論理削除） | 204 | Day 4 |
| 6 | 倉庫 | GET | /api/warehouses | 倉庫の一覧・検索 | 200 | Day 5 |
| 7 |  | GET | /api/warehouses/{id} | 倉庫の取得 | 200 | Day 5 |
| 8 |  | POST | /api/warehouses | 倉庫の登録 | 201 | Day 5 |
| 9 |  | PUT | /api/warehouses/{id} | 倉庫の更新 | 200 | Day 5 |
| 10 |  | DELETE | /api/warehouses/{id} | 倉庫の削除（論理削除） | 204 | Day 5 |
| 11 | ロケーション | GET | /api/locations | ロケーションの一覧・検索 | 200 | Day 5 |
| 12 |  | GET | /api/locations/{id} | ロケーションの取得 | 200 | Day 5 |
| 13 |  | POST | /api/locations | ロケーションの登録 | 201 | Day 5 |
| 14 |  | PUT | /api/locations/{id} | ロケーションの更新 | 200 | Day 5 |
| 15 |  | DELETE | /api/locations/{id} | ロケーションの削除（論理削除） | 204 | Day 5 |
| 16 | 入庫 | POST | /api/receipts | 入庫の登録 | 201 | Day 8 |
| 17 | 出庫 | POST | /api/issues | 出庫の登録 | 201 | Day 9 |
| 18 | 引当 | POST | /api/stocks/reserve | 在庫の引当 | 200 | Day 9 |
| 19 |  | POST | /api/stocks/release | 引当の解除 | 200 | Day 9 |
| 20 | 在庫照会 | GET | /api/stocks | 在庫の検索 | 200 | Day 11 |
| 21 |  | GET | /api/stocks/summary | 在庫の集計 | 200 | Day 11 |
| 22 | 在庫移動 | POST | /api/transfers | 在庫の移動 | 201 | Day 12 |
| 23 | 棚卸 | POST | /api/stocktakes | 棚卸ヘッダーの作成 | 201 | Day 13 |
| 24 |  | POST | /api/stocktakes/{id}/start | 棚卸の開始 | 200 | Day 13 |
| 25 |  | GET | /api/stocktakes | 棚卸の一覧 | 200 | Day 13 |
| 26 |  | GET | /api/stocktakes/{id} | 棚卸の取得（明細を含む） | 200 | Day 13 |
| 27 |  | POST | /api/stocktakes/{id}/lines | 棚卸明細の追加 | 201 | Day 13 |
| 28 |  | PUT | /api/stocktakes/{id}/lines/{lineId} | 実棚数量の入力・更新 | 200 | Day 13 |
| 29 |  | POST | /api/stocktakes/{id}/complete | 棚卸の確定 | 200 | Day 13 |
| 30 |  | POST | /api/stocktakes/{id}/cancel | 棚卸の中止 | 200 | Day 13 |
| 31 | 在庫下限アラート | GET | /api/stock-alerts | 下限を下回る商品の一覧 | 200 | Day 14 |
| 32 | 在庫変動履歴 | GET | /api/stock-movements | 履歴の検索 | 200 | Day 14 |
| 33 |  | GET | /api/stock-movements/{id} | 履歴の詳細取得 | 200 | Day 14 |
| 34 | CSV出力 | GET | /api/stock-movements/export | 履歴のCSV出力 | 200 | Day 15 |
| 35 |  | GET | /api/stocks/export | 在庫のCSV出力 | 200 | Day 15 |
| 36 | 運用 | GET | /health | 起動確認（ALBのヘルスチェック用） | 200 | Day 7 |

※ 実装状況は、各APIの実装・テスト結果に合わせて更新する。

---

## 4. API詳細

### 4.1 商品（products）

**レスポンスの項目（Product）**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 商品ID |
| productCode | 文字列 | 商品コード |
| name | 文字列 | 商品名 |
| description | 文字列（null可） | 商品説明 |
| price | 数値 | 価格（小数点以下2桁まで） |
| unit | 文字列 | 単位 |
| minStockQuantity | 整数（null可） | 最低在庫数量（在庫下限数量） |
| version | 整数 | バージョン |
| createdAt | 日時 | 作成日時 |
| updatedAt | 日時 | 更新日時 |

**GET /api/products（一覧・検索）**

| クエリ | 内容 |
|---|---|
| keyword | 商品コードまたは商品名の部分一致（任意） |
| page、pageSize | 2.6を参照 |

応答は、Productのページング形式。並び順は商品コード昇順。

**POST /api/products（登録）**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productCode | 文字列 | ○ | 50文字以内。削除済みも含めて一意 |
| name | 文字列 | ○ | 200文字以内 |
| description | 文字列 |  | 500文字以内 |
| price | 数値 | ○ | 0以上。小数点以下2桁まで（整数部8桁以内） |
| unit | 文字列 | ○ | 20文字以内 |
| minStockQuantity | 整数 |  | 0以上。省略時は未設定 |

エラー：400（`VALIDATION_ERROR`）、409（`DUPLICATE_CODE`）

**PUT /api/products/{id}（更新）**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| name | 文字列 | ○ | 200文字以内 |
| description | 文字列 |  | 500文字以内。nullで未設定に戻す |
| price | 数値 | ○ | 0以上。小数点以下2桁まで |
| unit | 文字列 | ○ | 20文字以内 |
| minStockQuantity | 整数 |  | 0以上。nullで未設定に戻す |
| version | 整数 | ○ | 取得時の値 |

商品コードは登録後に変更できない（案）。

エラー：400（`VALIDATION_ERROR`）、404（`NOT_FOUND`）、409（`VERSION_CONFLICT`）

更新成功時は、更新後のProductを返す。レスポンスには更新後の `version` を含める。

**GET /api/products/{id}（取得）**

指定した商品を1件取得する。

エラー：404（`NOT_FOUND`）

**DELETE /api/products/{id}（削除）**

論理削除する。

在庫数量または引当数量が残っている場合は削除できない。

エラー：404（`NOT_FOUND`）、409（`HAS_STOCK`）

---

### 4.2 倉庫（warehouses）

**レスポンスの項目（Warehouse）**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 倉庫ID |
| warehouseCode | 文字列 | 倉庫コード |
| name | 文字列 | 倉庫名 |
| address | 文字列（null可） | 住所 |
| description | 文字列（null可） | 備考 |
| version | 整数 | バージョン |
| createdAt | 日時 | 作成日時 |
| updatedAt | 日時 | 更新日時 |

| API | 入力 | 主なエラー |
|---|---|---|
| GET /api/warehouses | keyword（倉庫コード・倉庫名の部分一致）、page、pageSize | |
| GET /api/warehouses/{id} |  | 404（`NOT_FOUND`） |
| POST /api/warehouses | warehouseCode（必須、20文字以内）、name（必須、100文字以内）、address（任意、200文字以内）、description（任意、500文字以内） | 400（`VALIDATION_ERROR`）、409（`DUPLICATE_CODE`） |
| PUT /api/warehouses/{id} | name（必須）、address（任意）、description（任意）、version（必須）。倉庫コードは変更不可（案） | 400、404、409（`VERSION_CONFLICT`） |
| DELETE /api/warehouses/{id} |  | 404、409（`HAS_STOCK`、`STOCKTAKE_IN_PROGRESS`） |

倉庫削除時は、配下のロケーションに在庫数量または引当数量が残っていないことを確認する。

また、対象倉庫で棚卸が実施中の場合は削除できない。

---

### 4.3 ロケーション（locations）

**レスポンスの項目（Location）**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | ロケーションID |
| warehouseId | 整数 | 倉庫ID |
| locationCode | 文字列 | ロケーションコード |
| locationName | 文字列 | ロケーション名 |
| version | 整数 | バージョン |
| createdAt | 日時 | 作成日時 |
| updatedAt | 日時 | 更新日時 |

| API | 入力 | 主なエラー |
|---|---|---|
| GET /api/locations | warehouseId（任意）、keyword（ロケーションコード・ロケーション名の部分一致）、page、pageSize | |
| GET /api/locations/{id} |  | 404（`NOT_FOUND`） |
| POST /api/locations | warehouseId（必須）、locationCode（必須、30文字以内）、locationName（必須、100文字以内） | 400、404（倉庫が存在しない）、409（`DUPLICATE_CODE`、`STOCKTAKE_IN_PROGRESS`） |
| PUT /api/locations/{id} | locationName（必須）、version（必須）。倉庫とロケーションコードは変更不可（案） | 400、404、409（`VERSION_CONFLICT`） |
| DELETE /api/locations/{id} |  | 404、409（`HAS_STOCK`、`STOCKTAKE_IN_PROGRESS`） |

ロケーション登録時は、対象倉庫が存在し、削除されていないことを確認する。

また、対象倉庫で棚卸が実施中の場合は、ロケーションの追加・削除を禁止する。

ロケーションコードの重複は、同一倉庫内で判定する。削除済みのコードも重複判定の対象とする。

---

### 4.4 入庫・出庫・引当

どのAPIも、対象の倉庫で棚卸が実施中の場合は409（`STOCKTAKE_IN_PROGRESS`）を返す。

商品・ロケーションが存在しない（削除済みを含む）場合は404（`NOT_FOUND`）を返す。

在庫更新と棚卸中チェックは、同一トランザクション内で行う。具体的な行ロックの順序は `04_processing-design.md` を参照する。

**レスポンスの項目（Stock）**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 在庫ID |
| productId、productCode、productName | 整数・文字列 | 商品 |
| warehouseId、warehouseCode | 整数・文字列 | 倉庫 |
| locationId、locationCode | 整数・文字列 | ロケーション |
| quantity | 整数 | 在庫数量 |
| reservedQuantity | 整数 | 引当数量 |
| availableQuantity | 整数 | 引当可能数（在庫数量 − 引当数量） |
| version | 整数 | バージョン |

**POST /api/receipts（入庫）**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | 商品ID |
| locationId | 整数 | ○ | ロケーションID |
| quantity | 整数 | ○ | 1以上 |
| occurredAt | 日時 |  | 省略時はサーバーの現在日時 |
| remarks | 文字列 |  | 200文字以内 |

処理：

- 在庫の行がなければ作成し、数量を加算する。
- 在庫変動履歴（`RECEIPT`）を同一トランザクションで記録する。
- 在庫更新と履歴登録は同一トランザクションで実行する。

エラー：400（`VALIDATION_ERROR`）、404（`NOT_FOUND`）、409（`STOCKTAKE_IN_PROGRESS`）

応答：`movementId` と、更新後のStock。

HTTPステータス：201（Created）

**POST /api/issues（出庫）**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | 商品ID |
| locationId | 整数 | ○ | ロケーションID |
| quantity | 整数 | ○ | 1以上 |
| useReservation | 真偽値 |  | 引当済みの在庫から出庫するか。既定はfalse（案） |
| occurredAt | 日時 |  | 省略時はサーバーの現在日時 |
| remarks | 文字列 |  | 200文字以内 |

処理：

- `useReservation` がfalseのときは、引当可能数の範囲で出庫する。
- `useReservation` がtrueのときは、在庫数量と引当数量を同時に減らす（案）。
- 在庫変動履歴（`ISSUE`）を同一トランザクションで記録する。

※ `useReservation` の詳細仕様は未確定。現行実装とテスト結果を確認して確定する。

エラー：400（`VALIDATION_ERROR`）、404（`NOT_FOUND`）、409（`INSUFFICIENT_STOCK`、`STOCKTAKE_IN_PROGRESS`）

応答：`movementId` と、更新後のStock。

HTTPステータス：201（Created）

**POST /api/stocks/reserve（引当）、POST /api/stocks/release（引当解除）**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | 商品ID |
| locationId | 整数 | ○ | ロケーションID |
| quantity | 整数 | ○ | 1以上 |

処理：

- 引当は引当数量を加算する。
- 引当解除は引当数量を減算する。
- 引当可能数を超える引当はできない。
- 引当数量を超える解除はできない。
- 在庫変動履歴には記録しない。

エラー：

- 400（`VALIDATION_ERROR`）
- 404（`NOT_FOUND`）
- 409（引当は `INSUFFICIENT_STOCK`、解除は `INSUFFICIENT_RESERVATION`、どちらも `STOCKTAKE_IN_PROGRESS`）

応答：更新後のStock。

HTTPステータス：200（OK）

---

### 4.5 在庫照会・在庫下限アラート

**GET /api/stocks（在庫の検索）**

| クエリ | 内容 |
|---|---|
| productId、warehouseId、locationId | 絞り込み（任意） |
| keyword | 商品コードまたは商品名の部分一致（任意） |
| page、pageSize | 2.6を参照 |

応答は、Stockのページング形式。

並び順は商品コード、ロケーションコードの昇順。

**GET /api/stocks/summary（在庫の集計）**

| クエリ | 内容 |
|---|---|
| groupBy | `product`（商品別。全倉庫の合計）または `warehouse`（商品 × 倉庫別。ロケーションを合算） |
| keyword | 商品コードまたは商品名の部分一致（任意） |
| page、pageSize | 2.6を参照 |

応答の項目：

- productId
- productCode
- productName
- unit
- `warehouse` のときは warehouseId、warehouseCode
- totalQuantity
- totalReservedQuantity
- totalAvailableQuantity

集計値は、各ロケーションの在庫数量・引当数量を合算して算出する。

`availableQuantity` は `quantity - reservedQuantity` とする。

削除済みの商品・ロケーションに関連する在庫を集計対象に含めるかは、現時点では含めない案とする。

**GET /api/stock-alerts（下限アラート）**

全倉庫の在庫数量の合計が、商品の在庫下限数量を下回る商品を返す。

- 削除されていない商品を対象とする。
- 在庫下限数量が未設定の商品は対象外とする。
- 在庫が存在しない商品は在庫数量を0として扱う。
- 不足数量（`shortageQuantity`）は、在庫下限数量 − 在庫合計で計算する。

応答の項目：

- productId
- productCode
- productName
- totalQuantity
- minStockQuantity
- shortageQuantity（下限 − 在庫合計）

並び順は不足数量の降順とする（案）。

---

### 4.6 在庫移動

**POST /api/transfers**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | 商品ID |
| fromLocationId | 整数 | ○ | 移動元ロケーション |
| toLocationId | 整数 | ○ | 移動先ロケーション。移動元と同じ場合は400 |
| quantity | 整数 | ○ | 1以上 |
| occurredAt | 日時 |  | 省略時はサーバーの現在日時 |
| remarks | 文字列 |  | 200文字以内 |

処理：

- 移動元の在庫を減らし、移動先の在庫を増やす。
- 移動元の引当可能数が移動数量以上であることを確認する。
- 移動元・移動先の両方について、棚卸が実施中でないことを確認する。
- 在庫変動履歴は「移動出庫」と「移動入庫」の2行を、同じ移動番号で記録する。
- すべて1つのトランザクションで実行する。
- 移動元・移動先が複数倉庫にまたがる場合は、倉庫ID昇順で倉庫行をロックする。
- ロケーション行もID昇順でロックする。

移動元・移動先のどちらかの倉庫で棚卸が実施中の場合は409（`STOCKTAKE_IN_PROGRESS`）。

エラー：400（`VALIDATION_ERROR`）、404（`NOT_FOUND`）、409（`INSUFFICIENT_STOCK`、`STOCKTAKE_IN_PROGRESS`）

応答：

- `transferNo`
- 移動元のStock
- 移動先のStock

HTTPステータス：201（Created）

※ `transferNo` の採番方式は未確定。採番方式は `04_processing-design.md` の未決事項を参照する。

---

### 4.7 在庫変動履歴・CSV出力

**レスポンスの項目（StockMovement）**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 履歴ID |
| productId、productCode、productName | 整数・文字列 | 商品 |
| warehouseId、warehouseCode | 整数・文字列 | 倉庫 |
| locationId、locationCode | 整数・文字列 | ロケーション |
| movementType | 文字列 | 入出庫区分 |
| quantityChange | 整数 | 増減数量（符号付き） |
| transferNo | 整数（null可） | 移動番号 |
| occurredAt | 日時 | 発生日時 |
| remarks | 文字列（null可） | 備考 |
| createdAt | 日時 | 履歴登録日時（詳細レスポンス） |

**GET /api/stock-movements（履歴の検索）**

| クエリ | 内容 |
|---|---|
| productId、warehouseId、locationId | 絞り込み（任意） |
| movementType | 入出庫区分（任意） |
| from、to | 発生日時の範囲（任意） |
| transferNo | 移動番号（任意） |
| page、pageSize | 2.6を参照 |

並び順は発生日時の降順、同一日時の場合はIDの降順とする。

**GET /api/stock-movements/{id}（履歴の詳細取得）**

指定した在庫変動履歴を1件取得する。

詳細レスポンスには `createdAt` も含める。

対象が存在しない場合は404（`NOT_FOUND`）。

**GET /api/stock-movements/export、GET /api/stocks/export（CSV出力）**

検索APIと同じ絞り込み条件（ページングは除く）で、全件をCSVで返す。

| 項目 | 内容（案） |
|---|---|
| Content-Type | `text/csv` |
| 文字コード | UTF-8（BOM付き。Excelで開きやすくするため） |
| ヘッダー行 | あり |
| 列 | 各レスポンスの項目に対応する |

CSV出力は、検索APIと同じ条件・並び順とする。

大量データへの対応として、ストリーミング方式を採用する（案）。

出力中にデータ更新が発生した場合の整合性は未決事項とする。

---

### 4.8 棚卸（stocktakes）

#### 4.8.1 レスポンスの項目（Stocktake）

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 棚卸ID |
| warehouseId | 整数 | 対象倉庫ID |
| warehouseCode | 文字列 | 対象倉庫コード |
| stocktakeDate | 日付 | 棚卸日 |
| status | 文字列 | 状態（DRAFT、IN_PROGRESS、COMPLETED、CANCELLED） |
| completedAt | 日時（null可） | 確定日時 |
| remarks | 文字列（null可） | 備考 |
| version | 整数 | バージョン |

#### 4.8.2 棚卸明細（StocktakeLine）

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 棚卸明細ID |
| stockId | 整数（null可） | 棚卸開始時点の在庫ID。棚卸中に追加した明細はnull |
| productId | 整数 | 商品ID |
| productCode | 文字列 | 商品コード |
| productName | 文字列 | 商品名 |
| locationId | 整数 | ロケーションID |
| locationCode | 文字列 | ロケーションコード |
| bookQuantity | 整数 | 棚卸開始時点の帳簿数量 |
| actualQuantity | 整数（null可） | 実棚数量 |
| differenceQuantity | 整数（null可） | 差異数量（実棚数量 − 帳簿数量）。実棚数量が未入力の場合はnull |

※ `stocktake_lines` にVersionは持たせない。実棚数量の更新は棚卸ヘッダーの行ロックで排他制御する。

#### 4.8.3 POST /api/stocktakes（棚卸ヘッダーの作成）

リクエスト：

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| warehouseId | 整数 | ○ | 対象倉庫ID |
| stocktakeDate | 日付 | ○ | 棚卸日 |
| remarks | 文字列 |  | 200文字以内 |

処理：

- 棚卸ヘッダーを作成する。
- 作成時の状態は `DRAFT` とする。
- この時点では在庫数量の固定や棚卸明細の作成は行わない。
- 対象倉庫が存在しない場合は404（`NOT_FOUND`）を返す。
- 倉庫行をロックし、削除されていないことを確認する。
- 同じ倉庫に `IN_PROGRESS` の棚卸が存在する場合は409（`STOCKTAKE_IN_PROGRESS`）を返す（現行案）。
- DRAFT作成まで禁止するか、開始時のみ禁止するかは業務ルールとして要確認。

応答：

- 作成した棚卸ヘッダーを返す。
- HTTPステータスは201（Created）。

#### 4.8.4 POST /api/stocktakes/{id}/start（棚卸の開始）

リクエスト：

- リクエストボディなし

処理：

- 対象の棚卸ヘッダーが存在することを確認する。
- 対象倉庫の行をロックする。
- 倉庫が存在し、削除されていないことを確認する。
- 対象倉庫のロケーション行をID昇順でロックする。
- 棚卸ヘッダー行をロックし、最新の状態を取得する。
- 状態が `DRAFT` の場合のみ開始できる。
- 同じ倉庫に別の `IN_PROGRESS` の棚卸が存在しないことを確認する。
- 対象倉庫の在庫行を取得し、棚卸明細を作成する。
- 棚卸開始時点の在庫IDを `stockId` に保存する。
- 棚卸開始時点の在庫数量を `bookQuantity` に保存する。
- `actualQuantity` と `differenceQuantity` はnullで初期化する。
- 棚卸の状態を `IN_PROGRESS` に変更する。
- 入庫・出庫・引当・引当解除・在庫移動など、対象倉庫の在庫数量に影響する処理を制限する。
- 明細作成と状態変更は同一トランザクションで行う。

エラー：

- 404（`NOT_FOUND`）
- 409（`INVALID_STOCKTAKE_STATUS`：DRAFT以外の状態）
- 409（`STOCKTAKE_IN_PROGRESS`：同じ倉庫で別の棚卸が実施中）

応答：

- 更新後の棚卸ヘッダーを返す。
- HTTPステータスは200（OK）。

#### 4.8.5 GET /api/stocktakes（棚卸一覧）

| クエリ | 内容 |
|---|---|
| warehouseId | 倉庫IDによる絞り込み（任意） |
| status | 棚卸状態による絞り込み（任意） |
| page、pageSize | 2.6を参照 |

応答：

- 棚卸ヘッダーのページング形式。
- 並び順は棚卸日、作成日時の降順。

#### 4.8.6 GET /api/stocktakes/{id}（棚卸詳細）

処理：

- 棚卸ヘッダーと棚卸明細を取得する。
- 明細には商品情報、ロケーション情報、帳簿数量、実棚数量、差異数量を含める。
- 明細の並び順はロケーションコード、商品コードの昇順とする。

エラー：

- 404（`NOT_FOUND`）

応答：

- 棚卸ヘッダーと棚卸明細を返す。
- 棚卸明細は `lines` 配列に格納する。

#### 4.8.7 POST /api/stocktakes/{id}/lines（棚卸明細の追加）

リクエスト：

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | 商品ID |
| locationId | 整数 | ○ | ロケーションID |

処理：

- 対象の棚卸ヘッダーから倉庫IDを取得する。
- 倉庫行をロックする。
- 対象倉庫が存在し、削除されていないことを確認する。
- 対象ロケーション行をロックする。
- ロケーションが存在し、削除されていないことを確認する。
- 棚卸ヘッダー行をロックし、最新の状態を取得する。
- 棚卸状態が `IN_PROGRESS` の場合のみ追加できる。
- 指定された商品とロケーションが存在し、有効であることを確認する。
- 指定されたロケーションが棚卸対象倉庫に属していることを確認する。
- 同じ棚卸内に同一商品・同一ロケーションの明細が存在しないことを確認する。
- 対象の商品・ロケーションの在庫が既に存在する場合は、通常の棚卸開始時に作成された明細を使用するため追加できない。
- 棚卸中に帳簿に存在しない在庫を発見した場合に使用する。
- 追加した明細の `stockId` はnullとする。
- `bookQuantity` は0とする。
- `actualQuantity` と `differenceQuantity` はnullとする。
- 棚卸明細の追加は、在庫数量そのものを更新しない。

エラー：

- 400（`VALIDATION_ERROR`）
- 404（`NOT_FOUND`：棚卸、商品、ロケーションが存在しない）
- 409（`INVALID_STOCKTAKE_STATUS`：棚卸がIN_PROGRESSではない）
- 409（`DUPLICATE_STOCKTAKE_LINE`：同一商品・同一ロケーションの明細が既に存在する）
- 409（`STOCK_ALREADY_EXISTS`：対象の商品・ロケーションの在庫が既に存在する）

応答：

- 追加した棚卸明細を返す。
- HTTPステータスは201（Created）。

#### 4.8.8 PUT /api/stocktakes/{id}/lines/{lineId}（実棚数量の入力・更新）

リクエスト：

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| actualQuantity | 整数 | ○ | 0以上 |

処理：

- 棚卸ヘッダー行をロックする。
- 棚卸状態が `IN_PROGRESS` の場合のみ更新できる。
- 指定された棚卸明細が対象の棚卸に属していることを確認する。
- 実棚数量は0以上とする。
- `differenceQuantity` は実棚数量 − 帳簿数量で計算する。
- 実棚数量が0の場合も登録可能とする。
- 在庫数量はこのAPIでは更新しない。
- 在庫数量の更新は棚卸確定APIで行う。
- 棚卸明細にはVersionを持たせず、棚卸ヘッダー行のロックで排他制御する。

エラー：

- 400（`VALIDATION_ERROR`、`INVALID_ACTUAL_QUANTITY`）
- 404（`NOT_FOUND`：棚卸または棚卸明細が存在しない）
- 409（`INVALID_STOCKTAKE_STATUS`：棚卸がIN_PROGRESSではない）

応答：

- 更新後の棚卸明細を返す。
- HTTPステータスは200（OK）。

#### 4.8.9 POST /api/stocktakes/{id}/complete（棚卸の確定）

リクエスト：

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| version | 整数 | ○ | 棚卸ヘッダー取得時のversion |

処理：

- 棚卸ヘッダーが存在することを確認する。
- 対象倉庫の行をロックする。
- 対象倉庫のロケーション行をID昇順でロックする。
- 棚卸ヘッダー行をロックし、最新の状態を取得する。
- リクエストの `version` と現在の棚卸ヘッダーの `version` が一致することを確認する。
- 棚卸状態が `IN_PROGRESS` であることを確認する。
- すべての棚卸明細に実棚数量が入力されていることを確認する。
- 棚卸明細ごとに差異数量を計算する。
- `stockId` が設定されている明細は、棚卸開始時に存在していた在庫として扱う。
- `stockId` がnullの明細は、棚卸中に新たに発見した在庫として扱う。
- 対応する既存在庫行を在庫ID昇順でロックする。
- 在庫更新、新規在庫作成、在庫調整履歴の登録、棚卸状態の更新を同一トランザクションで実行する。

既存在庫の更新：

- 現在の在庫数量が棚卸開始時の帳簿数量（`bookQuantity`）と一致することを確認する。
- 一致しない場合は409（`STOCKTAKE_QUANTITY_CONFLICT`）を返す。
- 実棚数量が現在の引当数量を下回る場合は409（`RESERVED_QUANTITY_CONFLICT`）を返す。
- 在庫数量を実棚数量に更新する。
- 引当数量は変更しない。
- `stockId` が参照する在庫の商品ID・ロケーションIDが、棚卸明細の商品ID・ロケーションIDと一致することを確認する。

新規在庫の登録：

- `stockId` がnullの明細について、同じ商品・ロケーションの在庫が既に存在しないことを確認する。
- 既に存在する場合は409（`STOCK_ALREADY_EXISTS`）を返す。
- 実棚数量が1以上の場合は、新しい在庫行を作成する。
- 新規在庫の引当数量は0とする。
- 実棚数量が0の場合は、在庫行を作成しない。

在庫調整履歴：

- 差異数量が0以外の場合、在庫変動履歴に `STOCKTAKE_ADJUST` を登録する。
- 履歴の増減数量（`quantityChange`）には差異数量を設定する。
- 差異数量が0の場合は在庫調整履歴を登録しない。
- 新規在庫の場合も、実棚数量が1以上であれば帳簿数量0からの増加として履歴を登録する。

確定処理：

- 在庫数量の更新
- 新規在庫行の作成
- 在庫調整履歴の登録
- 棚卸状態の `COMPLETED` への変更
- 確定日時の登録
- 棚卸ヘッダーのVersion更新

上記の処理はすべて同一トランザクションで実行する。途中でエラーが発生した場合は、すべてロールバックする。

エラー：

- 400（`VALIDATION_ERROR`：version未指定など）
- 404（`NOT_FOUND`）
- 409（`VERSION_CONFLICT`：version不一致）
- 409（`INVALID_STOCKTAKE_STATUS`：IN_PROGRESS以外）
- 409（`STOCKTAKE_QUANTITY_CONFLICT`：帳簿数量と現在の在庫数量が不一致）
- 409（`RESERVED_QUANTITY_CONFLICT`：実棚数量が引当数量を下回る）
- 409（`STOCK_ALREADY_EXISTS`：新規在庫として追加する商品・ロケーションに在庫が既に存在する）
- 409（`ACTUAL_QUANTITY_NOT_ENTERED`：実棚数量が未入力の明細が存在する）

応答：

- 確定後の棚卸ヘッダーを返す。
- HTTPステータスは200（OK）。
- 更新後の `version` と `completedAt` を含める。

#### 4.8.10 POST /api/stocktakes/{id}/cancel（棚卸の中止）

リクエスト：

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| version | 整数 | ○ | 棚卸ヘッダー取得時のversion |

処理：

- 棚卸ヘッダーが存在することを確認する。
- 対象倉庫の行をロックする。
- 対象倉庫のロケーション行をID昇順でロックする。
- 棚卸ヘッダー行をロックし、最新の状態を取得する。
- リクエストの `version` と現在の棚卸ヘッダーの `version` が一致することを確認する。
- 棚卸状態が `IN_PROGRESS` の場合のみ中止できる。
- 棚卸状態を `CANCELLED` に変更する。
- 在庫数量の更新や在庫調整履歴の登録は行わない。
- 棚卸ヘッダーのVersionを更新する。
- 対象倉庫の棚卸による在庫操作制限を解除する。

エラー：

- 400（`VALIDATION_ERROR`：version未指定など）
- 404（`NOT_FOUND`）
- 409（`VERSION_CONFLICT`：version不一致）
- 409（`INVALID_STOCKTAKE_STATUS`：IN_PROGRESS以外）

応答：

- 中止後の棚卸ヘッダーを返す。
- HTTPステータスは200（OK）。

---

### 4.9 運用

**GET /health**

アプリが起動していれば200を返す。ALBのヘルスチェックに使う。

DB接続の確認を含めるかは実装時に決める。

---

## 5. 未決事項・変更履歴

### 5.1 未決事項

| 項目 | 決定予定 | 備考 |
|---|---|---|
| 出庫と引当の関係（`useReservation` の扱い） | 実装確認時 | 引当済みの在庫から出庫するときの動作 |
| 商品コード・倉庫コード・ロケーションコードの変更可否 | 実装確認時 | 現案は変更不可 |
| 棚卸明細の `stockId` と商品・ロケーションの整合チェック | 実装確認時 | 現案はアプリケーションで検証する（02_database-design.md の9章） |
| 棚卸ヘッダー作成時の同一倉庫IN_PROGRESSチェック | 実装確認時 | DRAFT作成まで禁止するか、開始時のみ禁止するか |
| CSVの文字コード | CSV実装時 | 現案はUTF-8（BOM付き） |
| CSV出力中のデータ更新に対する整合性 | CSV実装時 | スナップショットとして扱うかは未決 |
| `/health` にDB接続の確認を含めるか | 運用実装時 | |
| 在庫のVersion管理方法 | 実装確認時 | DB列、EF Core設定、Version更新処理を統一する |
| 棚卸・在庫更新の行ロック実装 | 実装確認時 | PostgreSQLの行ロックとロック順序を確認する |
| 排他制御失敗時の再試行 | 実装確認時 | 最大3回は案。再試行対象と対象外のエラーを整理する |
| 在庫移動番号（`transferNo`）の採番方法 | 在庫移動実装時 | DBシーケンス方式は案 |
| 削除済みマスタに関連する在庫の照会対象 | 在庫照会実装時 | 現案では含めない |
| 在庫下限アラートの並び順 | アラート実装時 | 不足数量の降順は案 |
| 商品削除と在庫更新の同時実行制御 | 実装確認時 | 商品削除は行ロックを行わない方針。詳細は04_processing-design.mdを参照 |
| 認証・権限 | 必要になった時点 | 現状は対象外 |

### 5.2 変更履歴

| 版 | 日付 | 内容 |
|---|---|---|
| 初版 | 2026-11-04 | 新規作成（共通仕様、API一覧、各APIの要点） |
| 改訂 | 2026-11-18 | 棚卸のAPIを、ヘッダー作成・開始・明細追加・実棚数量の入力・確定・中止に分割（4.8を差し替え）。API一覧と業務エラーコードに反映。商品（説明・価格）、倉庫（住所・備考）、ロケーション（locationName）の項目をDB設計書に合わせて更新。 |
| 改訂 | 2026-11-24 | 04_processing-design.mdとの整合性を確認。エラーコード、排他制御、棚卸処理のロック順序、削除時の棚卸中チェック、履歴の並び順・詳細項目、未決事項を更新。 |