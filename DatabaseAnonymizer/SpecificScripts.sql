update [AdventureWorks2019].[Person].[Person]
set FirstName = 'no data' ,
	MiddleName = 'no data' ,
	LastName = 'no data' 
where ModifiedDate < '20140101';
