Sample log4net Appender Configuration for Sync

	<appender name="SocketAppender" type="log4net.Appender.SocketAppender, Log4netSocketAppender">
	  <RemoteAddress value="192.168.99.100"/>
	  <RemotePort value="5000" />
	  <AddressFamily value="InterNetwork" />
	  <SocketType value="Stream" />
	  <ProtocolType value="Tcp" />
	  <ConAttemptsCount value="5" />
	  <ConAttemptsWaitingTimeMilliSeconds value="3000" />
	  <ReconnectTimeInSeconds value="500" />
	  <UseThreadPoolQueue value="true" />
	  <layout type="log4net.Layout.PatternLayout, log4net">
		<conversionPattern value="%property{log4net:HostName} %level %date{ISO8601} %thread %logger - %message%newline" />
	  </layout>
	</appender>

Sample log4net Socket Appender Configuration for Async

	<appender name="SocketAppender" type="log4net.Appender.SocketAppenderAsync, Log4netSocketAppender">
	  <RemoteAddress value="192.168.99.100"/>
	  <RemotePort value="5000" />
	  <AddressFamily value="InterNetwork" />
	  <SocketType value="Stream" />
	  <ProtocolType value="Tcp" />
	  <ConAttemptsCount value="2" />
	  <ConAttemptsWaitingTimeMilliSeconds value="500" />
	  <ReconnectTimeInSeconds value="60" />
	  <UseThreadPoolQueue value="false" />
	  <layout type="log4net.Layout.PatternLayout, log4net">
		<conversionPattern value="%property{log4net:HostName} %level %date{ISO8601} %thread %logger - %message%newline" />
	  </layout>
	</appender>