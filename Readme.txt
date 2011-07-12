For properly work you should add to your app.config or web.config file next:

	1. Add to configSections <section name="spolty.framework" type="Spolty.Framework.ConfigurationSections.SpoltyFrameworkSectionHandler, Spolty.Framework" />.

		Example:
			<configSections>
				<section name="spolty.framework" type="Spolty.Framework.ConfigurationSections.SpoltyFrameworkSectionHandler, Spolty.Framework" />
			</configSections>
	2. Add spolty.framework section. Add to element "factories" factory for work.  
	
	Examples:
		for Linq To SQL:
		<spolty.framework>
			<factories>
				<add name="Linq" type="Spolty.Framework.ExpressionMakers.Factories.LinqExpressionMakerFactory, Spolty.Framework"/>
			</factories>
		</spolty.framework>		
		
		for Entity Framework:
		<spolty.framework>
			<factories>
				<add name="EntityFramework" type="Spolty.Framework.ExpressionMakers.Factories.EntityFrameworkExpressionMakerFactory, Spolty.Framework"/>					
			</factories>
		</spolty.framework>
		
		if you use the both technology (Linq To Sql and Entity Framework) than in your config file add the both factory. And before create QueryDesigner you should
		change using factory. It can be done by setting name of factory to SpoltyFrameworkSectionHandler.Instance.Use
		
		Example:
		
		app.config
		
		<spolty.framework>
			<factories use="Linq">
				<add name="Linq" type="Spolty.Framework.ExpressionMakers.Factories.LinqExpressionMakerFactory, Spolty.Framework"/>
				<add name="EntityFramework" type="Spolty.Framework.ExpressionMakers.Factories.EntityFrameworkExpressionMakerFactory, Spolty.Framework"/>			
			</factories>
		</spolty.framework>		
		
		in code
		
		
		SpoltyFrameworkSectionHandler.Instance.Use = "EntityFramework";
		var queryDesigner = new QueryDesigner(someEFContext, typeof(SomeEFEntity));
		
		SpoltyFrameworkSectionHandler.Instance.Use = "Linq";
		var queryDesigner = new QueryDesigner(someLinqContext, typeof(SomeLinqEntity));
		
More information you could find here http://code.spolty.com
		