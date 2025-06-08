using System;
using System.Collections.Generic;
using System.Linq;

namespace kittywork.Wc3ReplayParser.Business;

public static class ReplayInfoExtensions
{
    public static IEnumerable<ReplayEvent> GetUnknownEvents(this ReplayInfo replayInfo)
    {
        if (replayInfo == null)
            throw new ArgumentNullException(nameof(replayInfo));

        return replayInfo.Events.Where(e => e.Action is UnknownAction);
    }
}
