# log4net Socket Appender (Logstash)
Socket (Tcp, Udp vs.) Appender For log4net (Logstash)

Nuget Package Url : https://www.nuget.org/packages/log4net.SocketAppender/

To install log4net Socket (Logstash) Appender, run the following command in the Package Manager Console

Install-Package log4net.SocketAppender

# Sample log4net Appender Configuration

	<appender name="SocketAppender" type="log4net.Appender.SocketAppender, Log4netSocketAppender">
	  <RemoteAddress value="192.168.99.100"/>
	  <RemotePort value="5000" />
	  <AddressFamily value="InterNetwork" />
	  <SocketType value="Stream" />
	  <ProtocolType value="Tcp" />
	  <ConAttemptsCount value="5" />
	  <ConAttemptsWaitingTimeMilliSeconds value="3000" />
	  <UseThreadPoolQueue value="true" />
	  <layout type="log4net.Layout.PatternLayout, log4net">
		<conversionPattern value="%property{log4net:HostName} %level %date{ISO8601} %thread %logger - %message%newline" />
	  </layout>
	</appender>
