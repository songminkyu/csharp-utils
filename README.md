# Util.Services

애플리케이션 전반에서 사용되는 공통 유틸리티 서비스들을 제공하는 라이브러리입니다.

## 📋 목차

- [개요](#개요)
- [서비스 목록](#서비스-목록)
- [설치 및 설정](#설치-및-설정)
- [사용법](#사용법)
- [기여하기](#기여하기)

## 🎯 개요

Util.Services는 WPF 애플리케이션 개발에 필요한 핵심 유틸리티들을 모듈화하여 제공합니다. 각 서비스는 독립적으로 사용 가능하며, 확장성과 재사용성을 고려하여 설계되었습니다.

## 🛠 서비스 목록

### 🔄 데이터 변환 서비스

#### BooleanToVisibilityConverter
WPF UI 바인딩을 위한 Boolean ↔ Visibility 변환기

```csharp
// XAML에서 사용
<TextBlock Visibility="{Binding IsVisible, Converter={StaticResource BooleanToVisibilityConverter}}" />

// Collapsed 모드 사용
<TextBlock Visibility="{Binding IsVisible, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=collapsed}" />
```

#### ConverterJson
JSON 직렬화/역직렬화 및 XML ↔ JSON 변환 서비스

```csharp
var converter = new ConverterJson();

// JSON 직렬화/역직렬화
string json = converter.SerializeToJson(myObject);
MyClass obj = converter.DeserializeFromJson<MyClass>(json);

// 배열 역직렬화
MyClass[] array = converter.DeserializeFromArrayJson<MyClass>(jsonArray);

// XML을 JSON으로 변환
MyClass result = converter.XmlToJson<MyClass>(xmlString);
```

### 🌐 HTTP 통신 서비스

#### HttpProvider
HTTP 요청을 위한 통합 서비스 (비동기 지원)

```csharp
var httpProvider = new HttpProvider(timeoutMinutes: 5);

// POST 요청
var response = await httpProvider.HttpSendMessage(requestData, "https://api.example.com", "utf-8");

// GET 요청 (단일 객체)
var result = await httpProvider.HTTPGetMessage<MyClass>("https://api.example.com/data");

// GET 요청 (배열)
var results = await httpProvider.HTTPGetMessages<MyClass>("https://api.example.com/list");

// URL 세그먼트 결합
string url = httpProvider.JoinUriSegments("https://api.example.com", "users", "123", "profile");
```

**특징:**
- HTTPS 인증서 검증 우회 옵션
- 커스텀 타임아웃 설정
- 자동 JSON 직렬화/역직렬화
- Thread-safe 비동기 처리

### 🔒 동기화 서비스

#### AsyncLock
고성능 비동기 뮤텍스 구현

```csharp
private static readonly AsyncLock mutex = new AsyncLock();

// 비동기 락
using (await mutex.LockAsync())
{
    // 임계 영역 코드
}

// 타임아웃이 있는 락 시도
bool success = await mutex.TryLockAsync(async () => {
    // 실행할 코드
}, TimeSpan.FromSeconds(5));

// 동기 락
using (mutex.Lock())
{
    // 임계 영역 코드
}
```

### 📊 데이터 바인딩 서비스

#### BindableBase
MVVM 패턴을 위한 고급 ObservableObject 구현

```csharp
public class MyViewModel : BindableBase
{
    // 속성 백킹 필드를 사용한 바인딩
    private string _name;
    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    // PropertyBag를 사용한 동적 바인딩
    public string Title
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    // 변경 콜백이 있는 바인딩
    public int Count
    {
        get => GetValue<int>();
        set => SetValue(value, OnCountChanged);
    }

    // Expression을 사용한 바인딩
    public string Status
    {
        get => GetValue(() => Status);
        set => SetValue(() => Status, value);
    }

    private void OnCountChanged()
    {
        // Count 변경 시 실행될 로직
    }
}
```

## 🚀 설치 및 설정

### 필수 요구사항

- .NET Framework 4.7.2+ 또는 .NET Core 3.1+
- Newtonsoft.Json
- CommunityToolkit.Mvvm

### 의존성 설치

```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

### 프로젝트 설정

```csharp
// Program.cs 또는 App.xaml.cs에서
using Util.Services;
```

## 📖 사용법

### 기본 사용 패턴

```csharp
// 1. 서비스 인스턴스 생성
var jsonConverter = new ConverterJson();
var httpProvider = new HttpProvider(5); // 5분 타임아웃

// 2. 비동기 작업 수행
var result = await httpProvider.HTTPGetMessage<MyData>("https://api.example.com");

// 3. 데이터 변환
var json = jsonConverter.SerializeToJson(result.ModelContext);
```

### MVVM 패턴에서의 활용

```csharp
public class MainViewModel : BindableBase
{
    private readonly HttpProvider _httpProvider;
    private readonly ConverterJson _jsonConverter;

    public MainViewModel()
    {
        _httpProvider = new HttpProvider();
        _jsonConverter = new ConverterJson();
    }

    public string Status
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public async Task LoadDataAsync()
    {
        Status = "Loading...";
        
        try
        {
            var result = await _httpProvider.HTTPGetMessage<MyData>("https://api.example.com");
            if (result.HttpStatusCode == 200)
            {
                Status = "Data loaded successfully";
                // 데이터 처리...
            }
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }
}
```

## 🔧 확장 및 커스터마이징

### 새로운 컨버터 추가

```csharp
public class CustomConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 변환 로직 구현
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 역변환 로직 구현
    }
}
```

### HTTP 프로바이더 확장

```csharp
public class ExtendedHttpProvider : HttpProvider
{
    public ExtendedHttpProvider() : base() { }

    public async Task<T> CustomRequestAsync<T>(string endpoint, Dictionary<string, string> headers)
    {
        // 커스텀 HTTP 요청 로직
    }
}
```

## 🏗 아키텍처 원칙

- **단일 책임 원칙**: 각 서비스는 하나의 명확한 책임을 가짐
- **개방-폐쇄 원칙**: 확장에는 열려있고 수정에는 닫혀있음
- **의존성 역전**: 인터페이스를 통한 느슨한 결합
- **비동기 우선**: 모든 I/O 작업은 비동기로 처리

## 🤝 기여하기

### 새로운 서비스 추가 가이드라인

1. **네이밍 규칙**: `[기능명]Provider` 또는 `[기능명]Service` 형태
2. **인터페이스 정의**: 공개 API는 인터페이스로 추상화
3. **비동기 지원**: I/O 작업이 포함된 경우 반드시 비동기 메서드 제공
4. **예외 처리**: 적절한 예외 처리 및 로깅
5. **단위 테스트**: 모든 공개 메서드에 대한 테스트 작성

### 코드 기여 절차

1. Fork 및 브랜치 생성
2. 기능 개발 및 테스트
3. 문서 업데이트
4. Pull Request 생성

## 📄 라이선스

이 프로젝트는 [MIT 라이선스](LICENSE) 하에 배포됩니다.

## 🆕 로드맵

- [ ] 캐싱 서비스 추가
- [ ] 로깅 서비스 통합
- [ ] 설정 관리 서비스
- [ ] 이벤트 버스 시스템
- [ ] 파일 I/O 헬퍼
- [ ] 암호화/복호화 서비스
- [ ] 데이터베이스 헬퍼
- [ ] 메모리 캐시 관리자

---

> 💡 **팁**: 각 서비스는 독립적으로 사용 가능하므로 필요한 부분만 선택하여 사용할 수 있습니다.