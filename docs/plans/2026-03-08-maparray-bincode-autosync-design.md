# MapArray → BinCode 자동 동기화 설계

**날짜:** 2026-03-08
**목표:** textBoxMapArray 입력 시 binCode를 실시간 자동 계산하여 textBoxBinCode에 반영, 저장 시에도 재계산하여 DB에 정확한 값 저장

---

## 배경

- MapArray 수정 탭은 active=1인 유일한 레코드만 조회 → 체크 항목은 항상 1건
- mapArray와 binCode는 동일 길이의 문자열, 각 인덱스가 1:1 대응

## 매핑 규칙

| mapArray[i] | binCode[i] |
|-------------|------------|
| `'0'`       | `'1'`      |
| `'2'`       | `'D'`      |
| 그 외        | 원본 binCode[i] 그대로 유지 |

## 컴포넌트

### 헬퍼 메서드: `ComputeBinCode`

```csharp
private string ComputeBinCode(string newMapArray, string origBinCode)
{
    if (string.IsNullOrEmpty(newMapArray) || string.IsNullOrEmpty(origBinCode)
        || newMapArray.Length != origBinCode.Length)
        return origBinCode;

    var sb = new StringBuilder(origBinCode);
    for (int i = 0; i < newMapArray.Length; i++)
    {
        if      (newMapArray[i] == '0') sb[i] = '1';
        else if (newMapArray[i] == '2') sb[i] = 'D';
        // 그 외: origBinCode[i] 그대로
    }
    return sb.ToString();
}
```

### 실시간 연동: `TextBoxMapArray_TextChanged` 수정

- 체크된 항목의 DataRow에서 `origBinCode` 추출
- `ComputeBinCode(newMapArray, origBinCode)` 호출
- `textBoxBinCode.Text = 계산값` 반영
- textBoxBinCode는 수동 override 가능하도록 읽기전용 전환 없음

### 저장 시 재계산: `BtnUpdateMapArray_Click` 수정

- `newMapArray`가 비어있지 않은 경우에만 실행
- 체크 항목의 DataRow에서 `origBinCode` 추출
- `ComputeBinCode(newMapArray, origBinCode)` 재계산
- 계산된 값으로 `newBinCode` override → `UpdateMapArrayDataAsync` 호출

## 변경 파일

- `Forms/MainForm.cs`
  - `ComputeBinCode` 헬퍼 추가 (MapArray region)
  - `TextBoxMapArray_TextChanged` 수정
  - `BtnUpdateMapArray_Click` 수정
