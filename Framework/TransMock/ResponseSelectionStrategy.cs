﻿
/***************************************
//   Copyright 2019 - Svetoslav Vasilev

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
*****************************************/

/// -----------------------------------------------------------------------------------------------------------
/// Module      :  ReceiveEndpoint.cs
/// Description :  This class represents a 1-way receive endpoint.
/// -----------------------------------------------------------------------------------------------------------

using TransMock.Communication.NamedPipes;

namespace TransMock
{
    /// <summary>
    /// A strategy describing the way to select a response for a message receievd from a mocked service
    /// in a 2-way communication scenario
    /// </summary>
    public abstract class ResponseSelectionStrategy
    {
        protected ResponseSelectionStrategy()
        {

        }

        public virtual MockMessage SelectResponseMessage(int requestIndex, MockMessage requestMessage)
        {
            return null;
        }
    }
}