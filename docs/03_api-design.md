# API設計書

| 項目 | 内容 |
|---|---|
| システム名 | AideaInventoryMng |
| 版 | 初版(ドラフト) |
| 作成日 | 2026-11-04 |
| 関連資料 | [01_system-overview.md](01_system-overview.md)(システム概要・構成設計書)、[02_database-design.md](02_database-design.md)(データベース設計書) |

※ 「(案)」と書かれた項目は、実装時に確認して確定する。

---

## 1. 概要

### 1.1 目的
AideaInventoryMngが提供するWeb APIの仕様を記載する。

### 1.2 位置づけ
- 本書は、共通仕様とAPI一覧、各APIの入力項目・業務ルール・エラーをまとめた初版である
- 実装に合わせて、Day 4〜14で各APIの詳細を追記し、Day 14に完成させる
- リクエスト・レスポンスの細かい型情報は、OpenAPI(実装から自動出力)を正とし、本書にはその要点を記載する

### 1.3 対象
第3章のAPI一覧に記載したAPI。認証・権限は対象外とする。

---

## 2. 共通仕様

### 2.1 基本

| 項目 | 内容 |
|---|---|
| 方式 | REST(HTTP + JSON) |
| ベースパス | `/api`(バージョニングは行わない。必要になった時点で `/api/v2` を追加する) |
| 文字コード | UTF-8 |
| パスの命名 | 小文字のkebab-case、複数形(例: `stock-movements`) |
| JSONのプロパティ名 | camelCase(例: `productCode`) |
| 認証 | なし(対象外。必要になった場合に追加する) |

### 2.2 値の形式

| 種類 | 形式 | 例 |
|---|---|---|
| 日時 | ISO 8601(UTC、`Z` 付き) | `2026-11-12T09:30:00Z` |
| 日付 | `yyyy-MM-dd` | `2026-11-20` |
| 数量 | 整数 | `10` |
| ID | 整数 | `1` |
| 区分値 | 英語の文字列(02_database-design.md の8章) | `RECEIPT` |

### 2.3 HTTPステータス

| ステータス | 意味 | 使う場面 |
|---|---|---|
| 200 OK | 成功 | 取得、更新、引当、確定など |
| 201 Created | 作成成功 | 登録(`Location` ヘッダに作成した資源のURLを返す) |
| 204 No Content | 成功(本文なし) | 削除 |
| 400 Bad Request | 入力エラー | 必須項目の欠落、形式・範囲の誤り |
| 404 Not Found | 対象が存在しない | 存在しないID、削除済みの資源 |
| 409 Conflict | 業務ルール違反・競合 | コード重複、バージョン不一致、在庫不足、棚卸中、削除不可など |
| 500 Internal Server Error | サーバー内部エラー | 想定外のエラー |

### 2.4 エラー応答
ASP.NET Coreの標準形式(ProblemDetails)に、業務エラーコード `code` を加えた形式とする(案)。

業務ルール違反の例(409)

```json
{
  "title": "在庫が不足しています。",
  "status": 409,
  "detail": "引当可能数は3です。",
  "code": "INSUFFICIENT_STOCK"
}
```

入力エラーの例(400)

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

### 2.5 業務エラーコード(案)

| コード | ステータス | 意味 |
|---|---|---|
| VALIDATION_ERROR | 400 | 入力値の誤り |
| NOT_FOUND | 404 | 対象が存在しない(削除済みを含む) |
| DUPLICATE_CODE | 409 | コードの重複(削除済みのコードも含む) |
| VERSION_CONFLICT | 409 | バージョン不一致(他の更新と競合) |
| INSUFFICIENT_STOCK | 409 | 在庫(引当可能数)が不足 |
| INSUFFICIENT_RESERVATION | 409 | 解除する数量が引当数量を超えている |
| HAS_STOCK | 409 | 在庫が残っているため削除できない |
| STOCKTAKE_IN_PROGRESS | 409 | 棚卸が実施中の倉庫のため、入庫・出庫・引当・移動ができない(または棚卸を開始できない) |
| INVALID_STATE | 409 | 状態が不正(確定済みの棚卸を再度確定するなど) |

### 2.6 ページング
一覧・検索APIに共通で、次のクエリパラメータを使う。

| パラメータ | 内容 | 既定値 |
|---|---|---|
| page | ページ番号(1始まり) | 1 |
| pageSize | 1ページの件数(最大100) | 20 |

応答は次の形式とする。

```json
{
  "items": [ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 53
}
```

### 2.7 排他制御
- 更新系のAPI(`PUT` と、棚卸の確定・中止)は、リクエストに `version` を必須とする
- `version` が現在の値と異なる場合は、409(`VERSION_CONFLICT`)を返す
- 更新に成功した応答には、更新後の `version` を含める

### 2.8 削除
- `DELETE` は論理削除とする(マスタのみ)
- 削除済みの資源は、一覧・取得の対象外とする(取得は404)
- 在庫が残っているものは削除できない(409、`HAS_STOCK`)

---

## 3. API一覧

| No | 機能 | メソッド | パス | 概要 | 成功時 | 実装予定 |
|---|---|---|---|---|---|---|
| 1 | 商品 | GET | /api/products | 商品の一覧・検索 | 200 | Day 4 |
| 2 | | GET | /api/products/{id} | 商品の取得 | 200 | Day 4 |
| 3 | | POST | /api/products | 商品の登録 | 201 | Day 4 |
| 4 | | PUT | /api/products/{id} | 商品の更新 | 200 | Day 4 |
| 5 | | DELETE | /api/products/{id} | 商品の削除(論理削除) | 204 | Day 4 |
| 6 | 倉庫 | GET | /api/warehouses | 倉庫の一覧・検索 | 200 | Day 5 |
| 7 | | GET | /api/warehouses/{id} | 倉庫の取得 | 200 | Day 5 |
| 8 | | POST | /api/warehouses | 倉庫の登録 | 201 | Day 5 |
| 9 | | PUT | /api/warehouses/{id} | 倉庫の更新 | 200 | Day 5 |
| 10 | | DELETE | /api/warehouses/{id} | 倉庫の削除(論理削除) | 204 | Day 5 |
| 11 | ロケーション | GET | /api/locations | ロケーションの一覧・検索 | 200 | Day 5 |
| 12 | | GET | /api/locations/{id} | ロケーションの取得 | 200 | Day 5 |
| 13 | | POST | /api/locations | ロケーションの登録 | 201 | Day 5 |
| 14 | | PUT | /api/locations/{id} | ロケーションの更新 | 200 | Day 5 |
| 15 | | DELETE | /api/locations/{id} | ロケーションの削除(論理削除) | 204 | Day 5 |
| 16 | 入庫 | POST | /api/receipts | 入庫の登録 | 201 | Day 8 |
| 17 | 出庫 | POST | /api/issues | 出庫の登録 | 201 | Day 9 |
| 18 | 引当 | POST | /api/stocks/reserve | 在庫の引当 | 200 | Day 9 |
| 19 | | POST | /api/stocks/release | 引当の解除 | 200 | Day 9 |
| 20 | 在庫照会 | GET | /api/stocks | 在庫の検索 | 200 | Day 11 |
| 21 | | GET | /api/stocks/summary | 在庫の集計 | 200 | Day 11 |
| 22 | 在庫移動 | POST | /api/transfers | 在庫の移動 | 201 | Day 12 |
| 23 | 棚卸 | POST | /api/stocktakes | 棚卸の開始 | 201 | Day 13 |
| 24 | | GET | /api/stocktakes | 棚卸の一覧 | 200 | Day 13 |
| 25 | | GET | /api/stocktakes/{id} | 棚卸の取得(明細を含む) | 200 | Day 13 |
| 26 | | PUT | /api/stocktakes/{id}/lines/{lineId} | 実数量の入力 | 200 | Day 13 |
| 27 | | POST | /api/stocktakes/{id}/complete | 棚卸の確定 | 200 | Day 13 |
| 28 | | POST | /api/stocktakes/{id}/cancel | 棚卸の中止 | 200 | Day 13 |
| 29 | 在庫下限アラート | GET | /api/stock-alerts | 下限を下回る商品の一覧 | 200 | Day 14 |
| 30 | 入出庫履歴 | GET | /api/stock-movements | 履歴の検索 | 200 | Day 14 |
| 31 | CSV出力 | GET | /api/stock-movements/export | 履歴のCSV出力 | 200 | Day 15 |
| 32 | | GET | /api/stocks/export | 在庫のCSV出力 | 200 | Day 15 |
| 33 | 運用 | GET | /health | 起動確認(ALBのヘルスチェック用) | 200 | Day 7 |

---

## 4. API詳細

### 4.1 商品(products)

**レスポンスの項目(Product)**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | |
| productCode | 文字列 | 商品コード |
| name | 文字列 | 商品名 |
| unit | 文字列 | 単位 |
| minStockQuantity | 整数(null可) | 在庫下限数量 |
| version | 整数 | バージョン |
| createdAt | 日時 | 作成日時 |
| updatedAt | 日時 | 更新日時 |

**GET /api/products(一覧・検索)**

| クエリ | 内容 |
|---|---|
| keyword | 商品コードまたは商品名の部分一致(任意) |
| page、pageSize | 2.6を参照 |

応答は、Productのページング形式。並び順は商品コード昇順。

**POST /api/products(登録)**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productCode | 文字列 | ○ | 50文字以内。削除済みも含めて一意 |
| name | 文字列 | ○ | 200文字以内 |
| unit | 文字列 | ○ | 20文字以内 |
| minStockQuantity | 整数 | | 0以上。省略時は未設定 |

エラー: 400(`VALIDATION_ERROR`)、409(`DUPLICATE_CODE`)

**PUT /api/products/{id}(更新)**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| name | 文字列 | ○ | 200文字以内 |
| unit | 文字列 | ○ | 20文字以内 |
| minStockQuantity | 整数 | | 0以上。nullで未設定に戻す |
| version | 整数 | ○ | 取得時の値 |

商品コードは登録後に変更できない(案)。
エラー: 400、404、409(`VERSION_CONFLICT`)

**GET /api/products/{id}(取得)**: エラーは404(`NOT_FOUND`)。

**DELETE /api/products/{id}(削除)**: 論理削除。エラーは404、409(`HAS_STOCK`: どこかのロケーションに在庫数量または引当数量が残っている)。

### 4.2 倉庫(warehouses)

**レスポンスの項目(Warehouse)**: id、warehouseCode、name、version、createdAt、updatedAt

| API | 入力 | 主なエラー |
|---|---|---|
| GET /api/warehouses | keyword(倉庫コード・倉庫名の部分一致)、page、pageSize | |
| GET /api/warehouses/{id} | | 404 |
| POST /api/warehouses | warehouseCode(必須、20文字以内)、name(必須、100文字以内) | 400、409(`DUPLICATE_CODE`) |
| PUT /api/warehouses/{id} | name(必須)、version(必須)。倉庫コードは変更不可(案) | 400、404、409(`VERSION_CONFLICT`) |
| DELETE /api/warehouses/{id} | | 404、409(`HAS_STOCK`: 配下のロケーションに在庫が残っている) |

### 4.3 ロケーション(locations)

**レスポンスの項目(Location)**: id、warehouseId、locationCode、name、version、createdAt、updatedAt

| API | 入力 | 主なエラー |
|---|---|---|
| GET /api/locations | warehouseId(任意)、keyword(ロケーションコード・名称の部分一致)、page、pageSize | |
| GET /api/locations/{id} | | 404 |
| POST /api/locations | warehouseId(必須)、locationCode(必須、30文字以内)、name(必須、100文字以内) | 400、404(倉庫が存在しない)、409(`DUPLICATE_CODE`: 同じ倉庫内で重複) |
| PUT /api/locations/{id} | name(必須)、version(必須)。倉庫とロケーションコードは変更不可(案) | 400、404、409(`VERSION_CONFLICT`) |
| DELETE /api/locations/{id} | | 404、409(`HAS_STOCK`) |

### 4.4 入庫・出庫・引当

どのAPIも、対象の倉庫で棚卸が実施中の場合は409(`STOCKTAKE_IN_PROGRESS`)を返す。商品・ロケーションが存在しない(削除済みを含む)場合は404を返す。

**レスポンスの項目(Stock)**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | |
| productId、productCode、productName | 整数・文字列 | 商品 |
| warehouseId、warehouseCode | 整数・文字列 | 倉庫 |
| locationId、locationCode | 整数・文字列 | ロケーション |
| quantity | 整数 | 在庫数量 |
| reservedQuantity | 整数 | 引当数量 |
| availableQuantity | 整数 | 引当可能数(在庫数量 − 引当数量) |
| version | 整数 | バージョン |

**POST /api/receipts(入庫)**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | |
| locationId | 整数 | ○ | |
| quantity | 整数 | ○ | 1以上 |
| occurredAt | 日時 | | 省略時はサーバーの現在日時 |
| remarks | 文字列 | | 200文字以内 |

処理: 在庫の行がなければ作成し、数量を加算する。入出庫履歴(RECEIPT)を同一トランザクションで記録する。
応答: `movementId` と、更新後のStock。

**POST /api/issues(出庫)**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | |
| locationId | 整数 | ○ | |
| quantity | 整数 | ○ | 1以上 |
| useReservation | 真偽値 | | 引当済みの在庫から出庫するか。既定はfalse(案) |
| occurredAt | 日時 | | 省略時はサーバーの現在日時 |
| remarks | 文字列 | | 200文字以内 |

処理: `useReservation` がfalseのときは、引当可能数の範囲で出庫する。trueのときは、在庫数量と引当数量を同時に減らす(案)。入出庫履歴(ISSUE)を同一トランザクションで記録する。
エラー: 400、404、409(`INSUFFICIENT_STOCK`、`STOCKTAKE_IN_PROGRESS`)
応答: `movementId` と、更新後のStock。

**POST /api/stocks/reserve(引当)、POST /api/stocks/release(引当解除)**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | |
| locationId | 整数 | ○ | |
| quantity | 整数 | ○ | 1以上 |

処理: 引当は引当数量を加算し、引当解除は減算する。入出庫履歴には記録しない。
エラー: 400、404、409(引当は `INSUFFICIENT_STOCK`、解除は `INSUFFICIENT_RESERVATION`、どちらも `STOCKTAKE_IN_PROGRESS`)
応答: 更新後のStock。

### 4.5 在庫照会・在庫下限アラート

**GET /api/stocks(在庫の検索)**

| クエリ | 内容 |
|---|---|
| productId、warehouseId、locationId | 絞り込み(任意) |
| keyword | 商品コードまたは商品名の部分一致(任意) |
| page、pageSize | 2.6を参照 |

応答は、Stockのページング形式。並び順は商品コード、ロケーションコードの昇順。

**GET /api/stocks/summary(在庫の集計)**

| クエリ | 内容 |
|---|---|
| groupBy | `product`(商品別。全倉庫の合計)または `warehouse`(商品 × 倉庫別。ロケーションを合算) |
| keyword、page、pageSize | |

応答の項目: productId、productCode、productName、unit、(`warehouse` のときは warehouseId、warehouseCode)、totalQuantity、totalReservedQuantity、totalAvailableQuantity

**GET /api/stock-alerts(下限アラート)**
全倉庫の在庫数量の合計が、商品の在庫下限数量を下回る商品を返す(下限が未設定の商品は対象外)。

応答の項目: productId、productCode、productName、totalQuantity、minStockQuantity、shortageQuantity(下限 − 在庫合計)

### 4.6 在庫移動

**POST /api/transfers**

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | |
| fromLocationId | 整数 | ○ | 移動元 |
| toLocationId | 整数 | ○ | 移動先。移動元と同じ場合は400 |
| quantity | 整数 | ○ | 1以上 |
| occurredAt | 日時 | | 省略時はサーバーの現在日時 |
| remarks | 文字列 | | 200文字以内 |

処理: 移動元の在庫を減らし、移動先の在庫を増やす。入出庫履歴は「移動出庫」と「移動入庫」の2行を、同じ移動番号で記録する。すべて1つのトランザクションで行う。移動元・移動先のどちらかの倉庫で棚卸が実施中の場合は409(`STOCKTAKE_IN_PROGRESS`)。
エラー: 400、404、409(`INSUFFICIENT_STOCK`、`STOCKTAKE_IN_PROGRESS`)
応答: `transferNo`、移動元のStock、移動先のStock。

### 4.7 入出庫履歴・CSV出力

**レスポンスの項目(StockMovement)**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | |
| productId、productCode、productName | 整数・文字列 | 商品 |
| warehouseId、warehouseCode | 整数・文字列 | 倉庫 |
| locationId、locationCode | 整数・文字列 | ロケーション |
| movementType | 文字列 | 入出庫区分 |
| quantityChange | 整数 | 増減数量(符号付き) |
| transferNo | 整数(null可) | 移動番号 |
| occurredAt | 日時 | 発生日時 |
| remarks | 文字列(null可) | 備考 |

**GET /api/stock-movements(履歴の検索)**

| クエリ | 内容 |
|---|---|
| productId、warehouseId、locationId | 絞り込み(任意) |
| movementType | 入出庫区分(任意) |
| occurredFrom、occurredTo | 発生日時の範囲(任意) |
| transferNo | 移動番号(任意) |
| page、pageSize | 2.6を参照 |

並び順は発生日時の降順。

**GET /api/stock-movements/export、GET /api/stocks/export(CSV出力)**
検索APIと同じ絞り込み条件(ページングは除く)で、全件をCSVで返す。

| 項目 | 内容(案) |
|---|---|
| Content-Type | `text/csv` |
| 文字コード | UTF-8(BOM付き。Excelで開きやすくするため) |
| ヘッダ行 | あり |
| 列 | 各レスポンスの項目に対応する |

### 4.8 棚卸

**レスポンスの項目(Stocktake)**

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | |
| warehouseId、warehouseCode | 整数・文字列 | 対象倉庫 |
| stocktakeDate | 日付 | 棚卸日 |
| status | 文字列 | 状態(IN_PROGRESS、COMPLETED、CANCELLED) |
| completedAt | 日時(null可) | 確定日時 |
| remarks | 文字列(null可) | 備考 |
| version | 整数 | バージョン |
| lines | 配列 | 棚卸明細(取得APIのみ) |

**棚卸明細(StocktakeLine)**: id、productId、productCode、productName、locationId、locationCode、bookQuantity、actualQuantity(null可)、differenceQuantity(実数量 − 帳簿数量。実数量が未入力のときはnull)

| API | 入力 | 処理・主なエラー |
|---|---|---|
| POST /api/stocktakes | warehouseId(必須)、stocktakeDate(必須)、remarks | 対象倉庫の全在庫行から、帳簿数量を写し取った明細を作成する(案)。同じ倉庫に実施中の棚卸がある場合は409(`STOCKTAKE_IN_PROGRESS`)(案)。倉庫がなければ404 |
| GET /api/stocktakes | warehouseId、status、page、pageSize | |
| GET /api/stocktakes/{id} | | 明細を含めて返す。404 |
| PUT /api/stocktakes/{id}/lines/{lineId} | actualQuantity(必須、0以上) | 実施中の棚卸のみ。実施中でなければ409(`INVALID_STATE`)。404 |
| POST /api/stocktakes/{id}/complete | version(必須) | 差異のある明細について、在庫数量を実数量に更新し、入出庫履歴(STOCKTAKE_ADJUST)を記録して、状態をCOMPLETEDにする。実数量が未入力の明細があれば確定できない(案)。409(`VERSION_CONFLICT`、`INVALID_STATE`) |
| POST /api/stocktakes/{id}/cancel | version(必須) | 状態をCANCELLEDにする。実施中以外は409(`INVALID_STATE`) |

### 4.9 運用

**GET /health**
アプリが起動していれば200を返す。ALBのヘルスチェックに使う。DB接続の確認を含めるかは実装時に決める。

---

## 5. 未決事項・変更履歴

### 5.1 未決事項

| 項目 | 決定予定 | 備考 |
|---|---|---|
| 出庫と引当の関係(`useReservation` の扱い) | Day 9 | 引当済みの在庫から出庫するときの動作 |
| 棚卸の対象明細の範囲 | Day 13 | 現案は対象倉庫の全在庫行 |
| 同じ倉庫で同時に実施中の棚卸を1つに制限するか | Day 13 | 現案は1つまで |
| 棚卸の確定時、実数量が未入力の明細がある場合 | Day 13 | 現案は確定不可 |
| 棚卸の確定時、実数量が引当数量を下回る場合 | Day 13 | 引当数量との整合の扱い |
| 棚卸で帳簿にない在庫を見つけた場合の明細追加API | Day 13 | 02_database-design.md の未決事項と同じ |
| 商品コード・倉庫コード・ロケーションコードの変更可否 | Day 4 | 現案は変更不可 |
| CSVの文字コード | Day 15 | 現案はUTF-8(BOM付き) |
| `/health` にDB接続の確認を含めるか | Day 7 | |
| 認証・権限 | 必要になった時点 | 現状は対象外 |

### 5.2 変更履歴

| 版 | 日付 | 内容 |
|---|---|---|
| 初版 | 2026-11-04 | 新規作成(共通仕様、API一覧、各APIの要点) |
