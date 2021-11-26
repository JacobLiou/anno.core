﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Anno.EngineData;

namespace Anno.Rpc.Server
{
    public class BusinessImpl : BrokerService.Iface
    {
        public string broker(Dictionary<string, string> input)
        {
            ActionResult actionResult;
            try
            {
                actionResult = Engine.Transmit(input);
            }
            catch (Exception ex)
            { //记录异常日志
                actionResult = new ActionResult
                {
                    Msg = ex.InnerException?.Message??ex.Message
                };
            }
            return JsonConvert.SerializeObject(actionResult);
        }
    }
}
