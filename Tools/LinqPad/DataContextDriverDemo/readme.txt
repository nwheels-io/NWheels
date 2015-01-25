BUILDING THIS PROJECT
=====================
Before building this project, add a reference to the LINQPad executable.
You must use LINQPad 1.37(beta) or later.

RUNNING THIS PROJECT
====================
There are two ways to deploy to your local machine. Either:
   - Zip up DataContextDriverDemo.dll; rename it to DataContextDriverDemo.lpx; go to LINQPad
      and click 'Add Driver', 'More Drivers', 'Browse...' and select the .lpx file
OR:
   - Edit the DevDeploy.bat file and call DevDeploy from the project post-build event

WHAT'S IN THIS PROJECT
======================
This project contains two demo drivers:

UniversalStaticDriver
---------------------
This static driver lets users query any type that looks or smells like a data context! It's an excellent
starting point for writing a static driver: you might find that all that's required is a little tweaking.

AstoriaDynamicDriver
---------------------
This is a dynamic driver for ADO.NET Data Services (3.5). It lets users specify a Data Services URI and
then it builds a typed context on the fly.

Please refer to the accompanying documentation ("Writing a LINQPad Data Context Driver.docx") for
more information.

Joseph Albahari