
### 4.8 棚卸(stocktakes)

#### 4.8.1 レスポンスの項目(Stocktake)

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 棚卸ID |
| warehouseId | 整数 | 対象倉庫ID |
| warehouseCode | 文字列 | 対象倉庫コード |
| stocktakeDate | 日付 | 棚卸日 |
| status | 文字列 | 状態(DRAFT、IN_PROGRESS、COMPLETED、CANCELLED) |
| completedAt | 日時(null可) | 確定日時 |
| remarks | 文字列(null可) | 備考 |
| version | 整数 | バージョン |

#### 4.8.2 棚卸明細(StocktakeLine)

| 項目 | 型 | 説明 |
|---|---|---|
| id | 整数 | 棚卸明細ID |
| stockId | 整数(null可) | 棚卸開始時点の在庫ID。棚卸中に追加した明細はnull |
| productId | 整数 | 商品ID |
| productCode | 文字列 | 商品コード |
| productName | 文字列 | 商品名 |
| locationId | 整数 | ロケーションID |
| locationCode | 文字列 | ロケーションコード |
| bookQuantity | 整数 | 棚卸開始時点の帳簿数量 |
| actualQuantity | 整数(null可) | 実棚数量 |
| differenceQuantity | 整数(null可) | 差異数量(実棚数量 − 帳簿数量)。実棚数量が未入力の場合はnull |

#### 4.8.3 POST /api/stocktakes(棚卸ヘッダーの作成)

リクエスト:

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| warehouseId | 整数 | ○ | 対象倉庫ID |
| stocktakeDate | 日付 | ○ | 棚卸日 |
| remarks | 文字列 | | 200文字以内 |

処理:

- 棚卸ヘッダーを作成する
- 作成時の状態は `DRAFT` とする
- この時点では在庫数量の固定や棚卸明細の作成は行わない
- 対象倉庫が存在しない場合は404(`NOT_FOUND`)を返す
- 同じ倉庫に `IN_PROGRESS` の棚卸が存在する場合は409(`STOCKTAKE_IN_PROGRESS`)を返す

応答:

- 作成した棚卸ヘッダーを返す
- HTTPステータスは201(Created)

#### 4.8.4 POST /api/stocktakes/{id}/start(棚卸の開始)

リクエスト:

- リクエストボディなし

処理:

- 対象の棚卸ヘッダーが存在することを確認する
- 状態が `DRAFT` の場合のみ開始できる
- 同じ倉庫に別の `IN_PROGRESS` の棚卸が存在しないことを確認する
- 対象倉庫の在庫行を取得し、棚卸明細を作成する
- 棚卸開始時点の在庫IDを `stockId` に保存する
- 棚卸開始時点の在庫数量を `bookQuantity` に保存する
- `actualQuantity` と `differenceQuantity` はnullで初期化する
- 対象倉庫の棚卸状態を `IN_PROGRESS` に変更する
- 入庫・出庫・引当・引当解除・在庫移動など、対象倉庫の在庫数量に影響する処理を制限する
- 明細作成と状態変更は同一トランザクションで行う

エラー:

- 404(`NOT_FOUND`)
- 409(`INVALID_STOCKTAKE_STATUS`: DRAFT以外の状態)
- 409(`STOCKTAKE_IN_PROGRESS`: 同じ倉庫で別の棚卸が実施中)

応答:

- 更新後の棚卸ヘッダーを返す
- HTTPステータスは200(OK)

#### 4.8.5 GET /api/stocktakes(棚卸一覧)

| クエリ | 内容 |
|---|---|
| warehouseId | 倉庫IDによる絞り込み(任意) |
| status | 棚卸状態による絞り込み(任意) |
| page、pageSize | 2.6を参照 |

応答:

- 棚卸ヘッダーのページング形式
- 並び順は棚卸日、作成日時の降順

#### 4.8.6 GET /api/stocktakes/{id}(棚卸詳細)

処理:

- 棚卸ヘッダーと棚卸明細を取得する
- 明細には商品情報、ロケーション情報、帳簿数量、実棚数量、差異数量を含める
- 明細の並び順はロケーションコード、商品コードの昇順とする

エラー:

- 404(`NOT_FOUND`)

応答:

- 棚卸ヘッダーと棚卸明細を返す
- 棚卸明細は `lines` 配列に格納する

#### 4.8.7 POST /api/stocktakes/{id}/lines(棚卸明細の追加)

リクエスト:

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| productId | 整数 | ○ | 商品ID |
| locationId | 整数 | ○ | ロケーションID |

処理:

- 棚卸状態が `IN_PROGRESS` の場合のみ追加できる
- 指定された商品とロケーションが存在し、有効であることを確認する
- 指定されたロケーションが棚卸対象倉庫に属していることを確認する
- 同じ棚卸内に同一商品・同一ロケーションの明細が存在しないことを確認する
- 対象の商品・ロケーションの在庫が既に存在する場合は、通常の棚卸開始時に作成された明細を使用するため追加できない
- 棚卸中に帳簿に存在しない在庫を発見した場合に使用する
- 追加した明細の `stockId` はnullとする
- `bookQuantity` は0とする
- `actualQuantity` と `differenceQuantity` はnullとする
- 棚卸明細の追加は、在庫数量そのものを更新しない

エラー:

- 400(`VALIDATION_ERROR`)
- 404(`NOT_FOUND`: 棚卸、商品、ロケーションが存在しない)
- 409(`INVALID_STOCKTAKE_STATUS`: 棚卸がIN_PROGRESSではない)
- 409(`DUPLICATE_STOCKTAKE_LINE`: 同一商品・同一ロケーションの明細が既に存在する)
- 409(`STOCK_ALREADY_EXISTS`: 対象の商品・ロケーションの在庫が既に存在する)

応答:

- 追加した棚卸明細を返す
- HTTPステータスは201(Created)

#### 4.8.8 PUT /api/stocktakes/{id}/lines/{lineId}(実棚数量の入力・更新)

リクエスト:

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| actualQuantity | 整数 | ○ | 0以上 |

処理:

- 棚卸状態が `IN_PROGRESS` の場合のみ更新できる
- 指定された棚卸明細が対象の棚卸に属していることを確認する
- 実棚数量は0以上とする
- `differenceQuantity` は実棚数量 − 帳簿数量で計算する
- 実棚数量が0の場合も登録可能とする
- 在庫数量はこのAPIでは更新しない
- 在庫数量の更新は棚卸確定APIで行う

エラー:

- 400(`VALIDATION_ERROR`、`INVALID_ACTUAL_QUANTITY`)
- 404(`NOT_FOUND`: 棚卸または棚卸明細が存在しない)
- 409(`INVALID_STOCKTAKE_STATUS`: 棚卸がIN_PROGRESSではない)

応答:

- 更新後の棚卸明細を返す
- HTTPステータスは200(OK)

#### 4.8.9 POST /api/stocktakes/{id}/complete(棚卸の確定)

リクエスト:

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| version | 整数 | ○ | 棚卸ヘッダー取得時のversion |

処理:

- 棚卸ヘッダーが存在することを確認する
- 棚卸状態が `IN_PROGRESS` であることを確認する
- リクエストの `version` と現在の棚卸ヘッダーの `version` が一致することを確認する
- すべての棚卸明細に実棚数量が入力されていることを確認する
- 棚卸明細ごとに差異数量を計算する
- `stockId` が設定されている明細は、棚卸開始時に存在していた在庫として扱う
- `stockId` がnullの明細は、棚卸中に新たに発見した在庫として扱う

既存在庫の更新:

- 現在の在庫数量が棚卸開始時の帳簿数量(`bookQuantity`)と一致することを確認する
- 一致しない場合は409(`STOCKTAKE_QUANTITY_CONFLICT`)を返す
- 実棚数量が現在の引当数量を下回る場合は409(`RESERVED_QUANTITY_CONFLICT`)を返す
- 在庫数量を実棚数量に更新する
- 引当数量は変更しない

新規在庫の登録:

- `stockId` がnullの明細について、同じ商品・ロケーションの在庫が既に存在しないことを確認する
- 実棚数量が1以上の場合は、新しい在庫行を作成する
- 新規在庫の引当数量は0とする
- 実棚数量が0の場合は、在庫行を作成しない

在庫調整履歴:

- 差異数量が0以外の場合、入出庫履歴に `STOCKTAKE_ADJUST` を登録する
- 履歴の増減数量(`quantityChange`)には差異数量を設定する
- 差異数量が0の場合は在庫調整履歴を登録しない
- 新規在庫の場合も、実棚数量が1以上であれば帳簿数量0からの増加として履歴を登録する

確定処理:

- 在庫数量の更新
- 新規在庫行の作成
- 在庫調整履歴の登録
- 棚卸状態の `COMPLETED` への変更
- 確定日時の登録

上記の処理はすべて同一トランザクションで実行する。途中でエラーが発生した場合は、すべてロールバックする。

エラー:

- 400(`VALIDATION_ERROR`: version未指定など)
- 404(`NOT_FOUND`)
- 409(`VERSION_CONFLICT`: version不一致)
- 409(`INVALID_STOCKTAKE_STATUS`: IN_PROGRESS以外)
- 409(`STOCKTAKE_QUANTITY_CONFLICT`: 帳簿数量と現在の在庫数量が不一致)
- 409(`RESERVED_QUANTITY_CONFLICT`: 実棚数量が引当数量を下回る)
- 409(`STOCK_ALREADY_EXISTS`: 新規在庫として追加する商品・ロケーションに在庫が既に存在する)
- 409(`INVALID_STATE`: 実棚数量が未入力の明細が存在する)

応答:

- 確定後の棚卸ヘッダーを返す
- HTTPステータスは200(OK)
- 更新後の `version` と `completedAt` を含める

#### 4.8.10 POST /api/stocktakes/{id}/cancel(棚卸の中止)

リクエスト:

| 項目 | 型 | 必須 | 制約・説明 |
|---|---|---|---|
| version | 整数 | ○ | 棚卸ヘッダー取得時のversion |

処理:

- 棚卸ヘッダーが存在することを確認する
- 棚卸状態が `IN_PROGRESS` の場合のみ中止できる
- リクエストの `version` と現在の棚卸ヘッダーの `version` が一致することを確認する
- 棚卸状態を `CANCELLED` に変更する
- 在庫数量の更新や在庫調整履歴の登録は行わない
- 対象倉庫の棚卸による在庫操作制限を解除する

エラー:

- 400(`VALIDATION_ERROR`: version未指定など)
- 404(`NOT_FOUND`)
- 409(`VERSION_CONFLICT`: version不一致)
- 409(`INVALID_STOCKTAKE_STATUS`: IN_PROGRESS以外)

応答:

- 中止後の棚卸ヘッダーを返す
- HTTPステータスは200(OK)
