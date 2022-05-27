# WPFMusic

WPF Music Player 프로젝트 입니다.
일부 Spectrum Analyzer 부분은 Jacob Johnston 코드를 참조 하였음을 알립니다.<br/>
(Note that the Spectrum Analyzer part refers to Jacob Johnston code.)<br/>
이 프로젝트는 WPF MVVM 아키텍처 및 기본 C#문법 WPF학습 목적으로 만들어졌습니다.

개발 환경 정보
-

- IDE : VS 2022
- Language : C# (WPF)
- Framework : .Net6 / Windows Only

사용 라이브러리
-

- 로그 관련
  - LogHelper / 자체 제작
- MVVM 관련
  - Microsoft.Toolkit.Mvvm / ver : 7.1.2
  - Microsoft.Xaml.Behaviors
- DependencyInjection 관련
  - Microsoft.Extensions.DependencyInjection / ver : 6.0.0
- Html Render
	- HtmlRenderer.WPF / ver : 1.5.0.6
- 음원 Tag 추출
	- taglib-sharp
- Bass
	- Bass.Net (only x86)


***

It is a simple wpf music player.<br/>
You can learn WPF MVVM architecture through this project.

Environment
-

- IDE : VS 2022
- Language : C# (WPF)
- Framework : .Net6 / Windows Only

library to use
-

- Log
  - LogHelper / self-made
- MVVM related
  - Microsoft.Toolkit.Mvvm/ver: 7.1.2
  - Microsoft.Xaml.Behaviors
- DependencyInjection related
  - Microsoft.Extensions.DependencyInjection/ver: 6.0.0
- Html Render
	- HtmlRenderer.WPF / ver : 1.5.0.6
- Extract music tag
	- taglib-sharp
- Bass
	- Bass.Net (only x86)

***



솔루션 구조
-

Model / View / ViewModel 모두 물리적 분리 목표


View -> Common 의존 참조 (외부에서 ViewModel 주입)<br/>
ViewModel -> Common, Model, Service 의존 참조<br/>
Service -> View 의존 참조 (popup window IoC 관리) <br/>
Model 의존 참조 없음 (단독 모듈)

구현 기능
-

- 기본 설정 기능
- 앨범 이미지 추출
- Tag 추출
- 기본 음악 재생
- 재생 리스트 관리
- 앨범 커버 고유 색상에 따라 배경색 지정
- 가사 보기

앞으로 구현 기능
-

- web url 재생 기능
- 라디오 Stream 재생 기능
- 동영상 플레이 관련 기능 추가

캡쳐 화면
-

#### `기본 화면`
///////


#### `앨범 커버 이미지 별 색상`
///////


#### `재생 화면`
///////


#### `가사 보기`
///////
