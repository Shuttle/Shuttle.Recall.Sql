﻿select isnull((select SequenceNumber from [dbo].[ProjectionPosition] where [Name] = @Name), 0) as SequenceNumber