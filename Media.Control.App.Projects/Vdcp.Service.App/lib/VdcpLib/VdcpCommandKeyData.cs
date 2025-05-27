using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    //class VdcpCommandKeyData
    //{
    //}

    public enum EumCommandKey
    {
       /// <summary>
       /// 아무것도 안닌경우 
       /// </summary>
       NORMAL,
       /// <summary>
       /// REMOTE/LOCAL 선택을 제외한 모든 기능 비활성화
       /// </summary>
       LOCALDISABLE,
       /// <summary>
       /// Controller의 지능 활성화
       /// </summary>
       LOCALENABLE,
       /// <summary>
       /// 선택 포트를 IDLE 상태로 만듬.
       /// </summary>
       STOP,
       /// <summary>
       /// 지정한 Clip을 재생 시킴 (Only OutPut Port)
       /// </summary>
       PLAY,
       /// <summary>
       /// 녹화 시작 (Only InPut Port)
       /// </summary>
       RECORD,
       /// <summary>
       /// ...
       /// </summary>
       FREEZE,
       /// <summary>
       /// 선택 포트의 Clip 재생을 일시 정지 시킴. (Only OutPut Port)
       /// </summary>
       STILL,
       /// <summary>
       /// Play,Still 상태의 Clip 다음 Frame으로 이동 (Only OutPut Port)
       /// </summary>
       STEP,
       /// <summary>
       /// 현재 상태를 종료 하고 계속 재생 (Only OutPut Port)
       /// </summary>
       CONTINUE,
       /// <summary>
       /// 지정한 frames 만큼 앞/뒤로 이동 (Only OutPut Port)
       /// </summary>
       JOG,
       /// <summary>
       /// 지정한 배속 Play (Only OutPut Port)
       /// 000000h = 정지
       /// 010000h = 정배속 앞으로 Play
       /// 7F0000h = 127 배속 까지 정배속 Play
       /// FF0000h = 정배속 뒤로 Play
       /// 800000h = 128 배속 까지 역배속 Play
       /// </summary>
       VARIPLAY,
       /// <summary>
       /// ...
       /// </summary>
       UNFREEZE,
       /// <summary>
       /// EE Mode 설정
       /// 0 - EE OFF
       /// 1 - EE ON
       /// 2 - AUTO
       /// </summary>
       EEMODE,
       /// <summary>
       /// Clip 명 변경하기
       /// (Clip 글자 수가 8자 이하인 경우)
       /// </summary>
       RENAMEID,
       /// <summary>
       /// Clip 명 변경하기
       /// (Clip 글자 수가 8자 이상인 경우)
       /// </summary>
       EXRENAMEID,
       /// <summary>
       /// 사용 된는 타이머의 표준 시간 설정
       /// </summary>
       PRESETTIME,
        /// <summary>
        /// 지정 포트의 통신 끊기
        /// </summary>
       CLOSEPORT,
       /// <summary>
       /// 열려 있는 포트를 선택 하기
       /// </summary>
       SELECTPORT,
       /// <summary>
       /// 녹화 준비 (Only Input Port) 
       /// [Clip 글자 수가 8자 이하인 경우]
       /// </summary>
       RECORDINIT,
       /// <summary>
       /// 녹화 준비 (Only Input Port) 
       /// [Clip 글자 수가 8자 이상인 경우]
       /// </summary>
       EXRECORDINIT,
       /// <summary>
       /// 재생 준비 (Only Output Port) 
       /// [Clip 글자 수가 8자 이하인 경우]
       /// </summary>
       PLAYCUE,
       /// <summary>
       ///  재생 준비 (Only Output Port) 
       ///  [Clip 글자 수가 8자 이상인 경우]
       /// </summary>
       EXPLAYCUE,
       /// <summary>
       /// 지정 시작/끝 위치까지 재생 준비 (Only Output Port) 
       /// [Clip 글자 수가 8자 이하인 경우]
       /// </summary>
       CUEWITHDATA,
       /// <summary>
       /// 지정 시작/끝 위치까지 재생 준비 (Only Output Port) 
       /// [Clip 글자 수가 8자 이상인 경우]
       /// </summary>
       EXCUEWITHDATA,
       /// <summary>
       /// 지정 Clip 삭제 하기
       /// [Clip 글자 수가 8자 이하인 경우]
       /// </summary>
       DELETEID,
       /// <summary>
       /// 지정 Clip 삭제 하기
       /// [Clip 글자 수가 8자 이상인 경우]
       /// </summary>
       EXDELETEID,
       /// <summary>
       /// 디스크에 있는 모든 파일 삭제
       /// </summary>
       CLEAR,
       /// <summary>
       /// 디스크의 사용 용량설정(%) 하기
       /// </summary>
       SIGNALFULL,
       /// <summary>
       /// 지정 Clip의 길이 만큼 녹화 준비 함 (Only Input Port)  
       /// [Clip 글자 수가 8자 이하인 경우]
       /// </summary>
       RECODEINITWITHDATA,
       /// <summary>
       /// 지정 Clip의 길이 만큼 녹화 준비 함 (Only Input Port)  
       /// [Clip 글자 수가 8자 이상인 경우]
       /// </summary>
       EXRECODEINITWITHDATA,
       /// <summary>
       /// ...
       /// </summary>
       PRESET,
       /// <summary>
       /// 재생및 녹화시 시작 대기 시간 설정
       /// </summary>
       DISKPREROLL,
       /// <summary>
       /// 지정 포트 오픈 하기
       /// [Output Port : 1 ~ 127 Input Port : -1 ~ -127]
       /// </summary>
       OPENPORT,
       /// <summary>
       /// List 명령 이후 Clip 10개식 전송
       /// [Clip 글자 수가 8자 이하인 경우]
       /// </summary>
       NEXT,
       /// <summary>
       /// List 명령 이후 Clip 10개식 전송
       /// [Clip 글자 수가 8자 이상인 경우]
       /// </summary>
       EXNEXT,
       /// <summary>
       /// 마지막 command 리턴
       /// </summary>
       LAST,
       //EXLAST,
       PORTSTATUS,
       /// <summary>
       /// 포트의 상태 정보를 리턴
       /// </summary>
       POSTIONREQUEST,
       /// <summary>
       /// 지금 재생 및 상태
       /// </summary>
       SYSTEMSTATUS,
       LIST,
       EXLIST,
       SIZEREQUEST,
       EXSIZEREQUEST,
       IDREQUEST,
       EXIDREQUEST,
        //Active ID Request
        ACTIVEIDREQUEST,
        EXACTIVEIDREQUEST,

        ERROR,
       ACK,
       NAK,

    }

}
