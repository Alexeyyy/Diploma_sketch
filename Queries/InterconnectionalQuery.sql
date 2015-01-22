/****** Script for SelectTopNRows command from SSMS  ******/
SELECT 
	fias.AOGUID,
	fias.CODE,
	kladrs.Kladr,
	kladrs.ComposedFullName,
	fias.FORMALNAME,
	fias.OFFNAME
FROM [fias].[ADDROBJ] AS fias
JOIN 
	(SELECT 
		[Kladr],
		[ComposedFullName]
	  FROM [hm_web_QA].[no].[cmn$Address]
	  WHERE LevelId = 10 AND Kladr IS NOT NULL --AND CHARINDEX('Димитровград', ComposedFullName, 0) > 0
	) AS kladrs
ON kladrs.Kladr = fias.CODE
ORDER BY fias.FORMALNAME

--fias.AOGUID
--fee76045-fe22-43a4-ad58-ad99e903bd58 - Ульяновская область
--73b29372-242c-42c5-89cd-8814bc2368af - г. Димитровград
--52456ed9-3883-4f67-a608-310a06ca0f9d - ул. Парадизова
--312c73ca-652a-4a7d-b95e-783e9d99ea10 - ул. Бурцева


--8920471


  
