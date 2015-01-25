rem
rem  You can simplify development by updating this batch file and then calling it from the 
rem  project's post-build event.
rem
rem  It copies the output .DLL (and .PDB) to LINQPad's drivers folder, so that LINQPad
rem  picks up the drivers immediately (without needing to click 'Add Driver').
rem
rem  The final part of the directory is the name of the assembly plus its public key token in brackets.

xcopy /i/y LinqPadODataV4Driver.* "%programdata%\LINQPad\Drivers\DataContext\4.0\LinqPadODataV4Driver (ce09d7e401f6d18e)\"
xcopy /i/y Hapil.* "%programdata%\LINQPad\Drivers\DataContext\4.0\LinqPadODataV4Driver (ce09d7e401f6d18e)\"
