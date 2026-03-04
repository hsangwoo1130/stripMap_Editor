using System;
using Serilog;
using TIBCO.Rendezvous;

namespace StripMapEditor.Utils
{
    /// <summary>
    /// TIBCO Rendezvous (RV) 통신 관리 클래스
    /// config.ini [RV] 섹션에서 Service/Network/Daemon/Subject 설정
    /// </summary>
    public class RvManager : IDisposable
    {
        // ─────────────────────────────────────────────
        // 설정 속성 (Program.cs에서 주입)
        // ─────────────────────────────────────────────
        public string Service        { get; set; }
        public string Network        { get; set; }
        public string Daemon         { get; set; }
        public string Subject        { get; set; }

        /// <summary>
        /// 시뮬레이션 모드 (true: TIBCO 없이 로그만 기록 — 테스트 용도)
        /// config.ini [RV] Simulation=true 로 활성화
        /// </summary>
        public bool SimulationMode { get; set; } = false;

        // ─────────────────────────────────────────────
        // RV 내부 객체
        // ─────────────────────────────────────────────
        private NetTransport _transport;
        private Queue        _queue;
        private QueueGroup   _queueGroup;
        private Message      _rvMsg;

        private bool _isInitialized = false;

        /// <summary>현재 연결 상태</summary>
        public bool IsConnected { get; private set; } = false;

        // ─────────────────────────────────────────────
        // 초기화 / 연결
        // ─────────────────────────────────────────────

        /// <summary>
        /// 랑데뷰 환경 초기화 (Queue/QueueGroup/Message 생성)
        /// Program 시작 시 1회 호출
        /// </summary>
        public bool RvInit()
        {
            if (SimulationMode)
            {
                _isInitialized = true;
                Log.Information("[RV_SIM] 시뮬레이션 모드 — TIBCO 초기화 건너뜀");
                return true;
            }

            try
            {
                TIBCO.Rendezvous.Environment.Open();

                _queueGroup = new QueueGroup();
                _queue      = new Queue();
                _queueGroup.Add(_queue);
                _rvMsg      = new Message();

                _isInitialized = true;
                Log.Information("[RV] 랑데뷰 초기화 완료");
                return true;
            }
            catch (RendezvousException ex)
            {
                Log.Error($"[RV] 랑데뷰 Open Error: {ex.Message} / 랑데뷰 설치 여부를 확인하세요.");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[RV] RvInit 예외: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Transport 생성 및 연결 (메시지 송신 전제 조건)
        /// </summary>
        public bool RvConnect()
        {
            if (SimulationMode)
            {
                IsConnected = true;
                Log.Information($"[RV_SIM] 시뮬레이션 모드 — 연결 건너뜀. Subject={Subject}");
                return true;
            }

            try
            {
                RvDestroyTransport();

                string daemon = Daemon ?? string.Empty;
                if (daemon.StartsWith("TCP:", StringComparison.OrdinalIgnoreCase))
                    daemon = daemon.Substring(4);

                _transport = new NetTransport(Service, Network, daemon);

                IsConnected = true;
                Log.Information($"[RV] 랑데뷰 연결 완료. Subject={Subject}");
                return true;
            }
            catch (RendezvousException ex)
            {
                Log.Error($"[RV] 랑데뷰 Transport 생성 에러: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[RV] RvConnect 예외: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────
        // 송신
        // ─────────────────────────────────────────────

        /// <summary>
        /// RV 메시지 전송 (필드명 "DATA"로 msgData 전송)
        /// </summary>
        public void RvSend(string subject, string msgData)
        {
            if (!IsConnected)
            {
                Log.Warning("[RV] 메시지를 보낼 수 없습니다. RV가 초기화되지 않았습니다.");
                return;
            }

            if (SimulationMode)
            {
                Log.Information($"[RV_SIM] 전송 시뮬레이션 TO:{subject} / DATA={msgData}");
                return;
            }

            if (_transport == null)
            {
                Log.Warning("[RV] 메시지를 보낼 수 없습니다. Transport가 없습니다.");
                return;
            }

            _rvMsg.Reset();
            _rvMsg.AddField("DATA", msgData);
            _rvMsg.SendSubject = subject;

            _transport.Send(_rvMsg);

            Log.Information($"[SEND RV] TO:{subject} / DATA={msgData}");
        }

        // ─────────────────────────────────────────────
        // 종료
        // ─────────────────────────────────────────────

        /// <summary>
        /// Transport 종료 (재연결 전 또는 앱 종료 시 호출)
        /// </summary>
        public void RvDestroyTransport()
        {
            if (_transport != null)
            {
                _transport.Destroy();
                _transport = null;
            }

            IsConnected = false;
        }

        /// <summary>
        /// 전체 RV 종료 — 앱 종료 시 호출
        /// </summary>
        public void RvTerminate()
        {
            if (!SimulationMode)
                RvDestroyTransport();
            else
                IsConnected = false;

            _rvMsg      = null;
            _queue      = null;
            _queueGroup = null;

            if (_isInitialized && !SimulationMode)
                TIBCO.Rendezvous.Environment.Close();

            _isInitialized = false;

            Log.Information("[RV] 랑데뷰 종료 완료");
        }

        public void Dispose() => RvTerminate();
    }
}
