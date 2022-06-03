# WPFMusic

WPF Music Player 프로젝트 입니다.
일부 Spectrum Analyzer 부분은 Jacob Johnston 코드를 참조 하였음을 알립니다.<br/>
(Note that the Spectrum Analyzer part refers to Jacob Johnston code.)<br/>
이 프로젝트는 WPF MVVM 아키텍처 및 기본 C#문법 WPF학습 목적으로 만들어졌습니다.

🛠️ 개발 환경 정보
-

- IDE : VS 2022
- Language : C# (WPF)


📁 What's included?
-

| Name| Framework | Build Status |
| --- | --- | --- | 
| **WPFMusic**<br />main application project | ![NET6](https://img.shields.io/badge/.NET-6.0-red)<br/>(Windows Only / x86) | None


📕 library to use
-

| Name | Version |
| --- | --- |
| **LogHelper**<br/>Log | ![NETFramework45](https://img.shields.io/badge/.NET%20Framework-4.5-orange)<br/>self-production
| **Microsoft.Toolkit.Mvvm**<br/>MVVM | 7.1.2 |
| **Microsoft.Xaml.Behaviors**<br/>MVVM |  |
| **Microsoft.Extensions.DependencyInjection**<br />DependencyInjection | 6.0.0 |
| **HtmlRenderer.WPF**<br />Html Render | 1.5.0.6 |
| **taglib-sharp**<br />Extract music tag | 2.0.3.7 |
| **Bass.Net**<br />audio library (only x86) | 2.4.7.1 |


***

It is a simple wpf music player.<br/>
You can learn WPF MVVM architecture through this project.

Environment
-

- IDE : VS 2022
- Language : C# (WPF)

***



💡 솔루션 구조
-

Model / View / ViewModel 모두 물리적 분리 목표


View -> Common 의존 참조 (외부에서 ViewModel 주입)<br/>
ViewModel -> Common, Model, Service 의존 참조<br/>
Service -> Common, Model 의존 참조
Model 의존 참조 없음 (단독 모듈)


✅ 구현 기능
-

- [x] 기본 설정 기능
- [x] 앨범 이미지 추출
- [x] Tag 추출
- [x] 기본 음악 재생
- [x] 재생 리스트 관리
- [x] 앨범 커버 고유 색상에 따라 배경색 지정
- [x] 가사 보기


☑️ 앞으로 구현 기능
-

- [ ] web url 재생 기능
- [ ] 사이즈에 따른 반응형 화면 처리<br/>(사이즈가 큰 경우, 전체 화면에서는 우측으로 플레이리스트 표시)
- [ ] 유튜브 뮤직 / 라디오 Stream 재생 기능
- [ ] 동영상 플레이 관련 기능 추가


📷 Screenshots
-

#### `기본 화면`
![image](https://user-images.githubusercontent.com/13028129/170911190-898f412f-8e41-469a-93bd-b8bbb55f4df7.png)


#### `앨범 커버 이미지 별 색상`
| ![image](https://user-images.githubusercontent.com/13028129/170911322-23f02d47-578d-4a0e-b0b5-ee067170e0a9.png) | ![image](https://user-images.githubusercontent.com/13028129/170911334-2f90a9bc-8c18-4433-9676-a7836b8f1ee0.png) |
| --- | --- |


#### `재생 화면`
![image](https://user-images.githubusercontent.com/13028129/170911391-610d5899-46c8-4b22-9723-86bf83347e44.png)


#### `가사 보기`
![image](https://user-images.githubusercontent.com/13028129/170911399-355bb019-867f-4b2d-bd87-8bd54d4a982d.png)


Special thanks
-

- [🔗 powe0101](https://github.com/powe0101)


License
-

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions: 

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE. 
