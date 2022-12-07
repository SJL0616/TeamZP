게임 실행 방법 및 개발 스크립트 관련 설명. 
작성자 이상준


1.게임 실행방법: 
   유니티 프로젝트 내에서:  scMain에서 게임 스타트 버튼 클릭.
   오류 대응방법:
    - 빌드 세팅이 초기화 된 경우  :   Build Settings에 씬을 추가 => 0. scMain / 1. scLobby / 2. scRoom / 3. scNetPlay 순서.
    - 포톤 네트워크 오류가 날 경우 : 
           Assets\Photon Unity Networking\Resources 위치의 PhotonServerSettings 파일을 세팅.
           = > 1. 인스펙터 상단의 Hosting을 "Best Region"으로 설정
           = > 2. AppId 에 ec7b3c8b-a2c6-44d2-90e3-31f3f302e3ee 입력
           = > 3. 인스펙터 중간 부분의 ClientSettings 바로 아래에 있는  "Auto-Join Lobby" 체크.
           = > 4. 재실행.
           (인스펙터 이미지 첨부함)


2. 스크립트 경로 및 기타 설명 
스크립트 폴더 경로: Assets\2.Scripts\Character,Item
씬 scNetPlay에서 쓰이는 스크립트 위주로 주석을 작성하였습니다.
( 2.Scripts 파일에 있는 StageManager에도 설명을 추가하였습니다.)
