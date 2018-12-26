﻿using JT808.DotNetty.Abstractions.Dtos;
using JT808.DotNetty.Core.Handlers;
using JT808.DotNetty.Core.Interfaces;
using JT808.DotNetty.Core.Metadata;
using JT808.DotNetty.Core.Services;
using Newtonsoft.Json;

namespace JT808.DotNetty.WebApi.Handlers
{
    /// <summary>
    /// 默认消息处理业务实现
    /// </summary>
    public class JT808MsgIdDefaultWebApiHandler : JT808MsgIdHttpHandlerBase
    {
        private const string sessionRoutePrefix = "Session";

        private const string transmitPrefix = "Transmit";

        private readonly JT808TcpAtomicCounterService jT808TcpAtomicCounterService;

        private readonly JT808UdpAtomicCounterService jT808UdpAtomicCounterService;

        private readonly JT808TransmitAddressFilterService jT808TransmitAddressFilterService;

        private readonly IJT808TcpSessionService jT808TcpSessionService;

        private readonly IJT808UnificationTcpSendService jT808UnificationTcpSendService;

        private readonly IJT808UnificationUdpSendService jT808UnificationUdpSendService;

        /// <summary>
        /// TCP一套注入
        /// </summary>
        /// <param name="jT808TcpAtomicCounterService"></param>
        public JT808MsgIdDefaultWebApiHandler(
            IJT808UnificationTcpSendService jT808UnificationTcpSendService,
            IJT808TcpSessionService jT808TcpSessionService,
            JT808TransmitAddressFilterService jT808TransmitAddressFilterService,
            JT808TcpAtomicCounterService jT808TcpAtomicCounterService
            )
        {
            this.jT808UnificationTcpSendService = jT808UnificationTcpSendService;
            this.jT808TcpSessionService = jT808TcpSessionService;
            this.jT808TransmitAddressFilterService = jT808TransmitAddressFilterService;
            this.jT808TcpAtomicCounterService = jT808TcpAtomicCounterService;
            InitTcpRoute();
        }

        /// <summary>
        /// UDP一套注入
        /// </summary>
        /// <param name="jT808UdpAtomicCounterService"></param>
        public JT808MsgIdDefaultWebApiHandler(
            IJT808UnificationUdpSendService jT808UnificationUdpSendService,
            JT808UdpAtomicCounterService jT808UdpAtomicCounterService
            )
        {
            this.jT808UnificationUdpSendService = jT808UnificationUdpSendService;
            this.jT808UdpAtomicCounterService = jT808UdpAtomicCounterService;
            InitUdpRoute();
        }

        /// <summary>
        /// 统一的一套注入
        /// </summary>
        /// <param name="jT808TcpAtomicCounterService"></param>
        /// <param name="jT808UdpAtomicCounterService"></param>
        public JT808MsgIdDefaultWebApiHandler(
             IJT808UnificationTcpSendService jT808UnificationTcpSendService,
             IJT808UnificationUdpSendService jT808UnificationUdpSendService,
             IJT808TcpSessionService jT808TcpSessionService,
             JT808TransmitAddressFilterService jT808TransmitAddressFilterService,
             JT808TcpAtomicCounterService jT808TcpAtomicCounterService,
             JT808UdpAtomicCounterService jT808UdpAtomicCounterService
           )
        {
            this.jT808UnificationTcpSendService = jT808UnificationTcpSendService;
            this.jT808UnificationUdpSendService = jT808UnificationUdpSendService;
            this.jT808TcpSessionService = jT808TcpSessionService;
            this.jT808TransmitAddressFilterService = jT808TransmitAddressFilterService;
            this.jT808TcpAtomicCounterService = jT808TcpAtomicCounterService;
            this.jT808UdpAtomicCounterService = jT808UdpAtomicCounterService;
            InitTcpRoute();
            InitUdpRoute();
        }

        /// <summary>
        /// 会话服务集合
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse GetSessionAll(JT808HttpRequest request)
        {
            var result = jT808TcpSessionService.GetAll();
            return CreateJT808HttpResponse(result);
        }

        /// <summary>
        /// 会话服务-通过设备终端号移除对应会话
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse RemoveByTerminalPhoneNo(JT808HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Json))
            {
                return EmptyHttpResponse();
            }
            var result = jT808TcpSessionService.RemoveByTerminalPhoneNo(request.Json);
            return CreateJT808HttpResponse(result);
        }

        /// <summary>
        /// 添加转发过滤地址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse AddTransmitAddress(JT808HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Json))
            {
                return EmptyHttpResponse();
            }
            JT808IPAddressDto jT808IPAddressDto = JsonConvert.DeserializeObject<JT808IPAddressDto>(request.Json);
            return CreateJT808HttpResponse(jT808TransmitAddressFilterService.Add(jT808IPAddressDto));
        }

        /// <summary>
        /// 删除转发过滤地址（不能删除在网关服务器配置文件配的地址）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse RemoveTransmitAddress(JT808HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Json))
            {
                return EmptyHttpResponse();
            }
            JT808IPAddressDto jT808IPAddressDto = JsonConvert.DeserializeObject<JT808IPAddressDto>(request.Json);
            return CreateJT808HttpResponse(jT808TransmitAddressFilterService.Remove(jT808IPAddressDto));
        }

        /// <summary>
        /// 获取转发过滤地址信息集合
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse GetTransmitAll(JT808HttpRequest request)
        {
            return CreateJT808HttpResponse(jT808TransmitAddressFilterService.GetAll());
        }

        /// <summary>
        /// 获取Tcp包计数器
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse GetTcpAtomicCounter(JT808HttpRequest request)
        {
            JT808AtomicCounterDto jT808AtomicCounterDto = new JT808AtomicCounterDto();
            jT808AtomicCounterDto.MsgFailCount = jT808TcpAtomicCounterService.MsgFailCount;
            jT808AtomicCounterDto.MsgSuccessCount = jT808TcpAtomicCounterService.MsgSuccessCount;
            return CreateJT808HttpResponse(new JT808ResultDto<JT808AtomicCounterDto>
            {
                Code = JT808ResultCode.Ok,
                Data = jT808AtomicCounterDto
            });
        }

        /// <summary>
        /// 获取Udp包计数器
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse GetUdpAtomicCounter(JT808HttpRequest request)
        {
            JT808AtomicCounterDto jT808AtomicCounterDto = new JT808AtomicCounterDto();
            jT808AtomicCounterDto.MsgFailCount = jT808UdpAtomicCounterService.MsgFailCount;
            jT808AtomicCounterDto.MsgSuccessCount = jT808UdpAtomicCounterService.MsgSuccessCount;
            return CreateJT808HttpResponse(new JT808ResultDto<JT808AtomicCounterDto>
            {
                Code = JT808ResultCode.Ok,
                Data = jT808AtomicCounterDto
            });
        }

        /// <summary>
        /// 基于Tcp的统一下发信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse UnificationTcpSend(JT808HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Json))
            {
                return EmptyHttpResponse();
            }
            JT808UnificationSendRequestDto jT808UnificationSendRequestDto = JsonConvert.DeserializeObject<JT808UnificationSendRequestDto>(request.Json);
            var result = jT808UnificationTcpSendService.Send(jT808UnificationSendRequestDto.TerminalPhoneNo, jT808UnificationSendRequestDto.Data);
            return CreateJT808HttpResponse(result);
        }

        /// <summary>
        /// 基于Udp的统一下发信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public JT808HttpResponse UnificationUdpSend(JT808HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Json))
            {
                return EmptyHttpResponse();
            }
            JT808UnificationSendRequestDto jT808UnificationSendRequestDto = JsonConvert.DeserializeObject<JT808UnificationSendRequestDto>(request.Json);
            var result = jT808UnificationUdpSendService.Send(jT808UnificationSendRequestDto.TerminalPhoneNo, jT808UnificationSendRequestDto.Data);
            return CreateJT808HttpResponse(result);
        }

        protected virtual void InitTcpRoute()
        {
            CreateRoute($"{transmitPrefix}/Add", AddTransmitAddress);
            CreateRoute($"{transmitPrefix}/Remove", RemoveTransmitAddress);
            CreateRoute($"{transmitPrefix}/GetAll", GetTransmitAll);
            CreateRoute($"GetTcpAtomicCounter", GetTcpAtomicCounter);
            CreateRoute($"{sessionRoutePrefix}/GetAll", GetSessionAll);
            CreateRoute($"{sessionRoutePrefix}/RemoveByTerminalPhoneNo", RemoveByTerminalPhoneNo);
            CreateRoute($"UnificationTcpSend", UnificationTcpSend);
        }

        protected virtual void InitUdpRoute()
        {
            CreateRoute($"GetUdpAtomicCounter", GetUdpAtomicCounter);
            CreateRoute($"UnificationUdpSend", UnificationUdpSend);
        }
    }
}