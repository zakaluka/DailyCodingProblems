@echo on

.paket\paket.exe restore

.paket\paket.exe install

fake run
