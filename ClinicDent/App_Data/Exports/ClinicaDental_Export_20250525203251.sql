-- Exportación SQL de ClinicaDental
-- Fecha: 2025-05-25 20:32:51
SET FOREIGN_KEY_CHECKS=0;

-- Datos de la tabla Citas
SELECT * INTO #TempCitas FROM [Citas];

-- Datos de la tabla Consulta
SELECT * INTO #TempConsulta FROM [Consulta];

-- Datos de la tabla Consultas_Tratamientos
SELECT * INTO #TempConsultas_Tratamientos FROM [Consultas_Tratamientos];

-- Datos de la tabla Dentistas
SELECT * INTO #TempDentistas FROM [Dentistas];

-- Datos de la tabla Historial_Clinico
SELECT * INTO #TempHistorial_Clinico FROM [Historial_Clinico];

-- Datos de la tabla Historial_Pagos
SELECT * INTO #TempHistorial_Pagos FROM [Historial_Pagos];

-- Datos de la tabla Inventario
SELECT * INTO #TempInventario FROM [Inventario];

-- Datos de la tabla Materiales
SELECT * INTO #TempMateriales FROM [Materiales];

-- Datos de la tabla Pacientes
SELECT * INTO #TempPacientes FROM [Pacientes];

-- Datos de la tabla Pagos
SELECT * INTO #TempPagos FROM [Pagos];

-- Datos de la tabla Pagos_Cuotas
SELECT * INTO #TempPagos_Cuotas FROM [Pagos_Cuotas];

-- Datos de la tabla Roles
SELECT * INTO #TempRoles FROM [Roles];

-- Datos de la tabla Tipo_Cobro
SELECT * INTO #TempTipo_Cobro FROM [Tipo_Cobro];

-- Datos de la tabla Tratamientos
SELECT * INTO #TempTratamientos FROM [Tratamientos];

-- Datos de la tabla Usuarios
SELECT * INTO #TempUsuarios FROM [Usuarios];

-- Inserciones para la tabla Citas
INSERT INTO [Citas] SELECT * FROM #TempCitas;
DROP TABLE #TempCitas;

-- Inserciones para la tabla Consulta
INSERT INTO [Consulta] SELECT * FROM #TempConsulta;
DROP TABLE #TempConsulta;

-- Inserciones para la tabla Consultas_Tratamientos
INSERT INTO [Consultas_Tratamientos] SELECT * FROM #TempConsultas_Tratamientos;
DROP TABLE #TempConsultas_Tratamientos;

-- Inserciones para la tabla Dentistas
INSERT INTO [Dentistas] SELECT * FROM #TempDentistas;
DROP TABLE #TempDentistas;

-- Inserciones para la tabla Historial_Clinico
INSERT INTO [Historial_Clinico] SELECT * FROM #TempHistorial_Clinico;
DROP TABLE #TempHistorial_Clinico;

-- Inserciones para la tabla Historial_Pagos
INSERT INTO [Historial_Pagos] SELECT * FROM #TempHistorial_Pagos;
DROP TABLE #TempHistorial_Pagos;

-- Inserciones para la tabla Inventario
INSERT INTO [Inventario] SELECT * FROM #TempInventario;
DROP TABLE #TempInventario;

-- Inserciones para la tabla Materiales
INSERT INTO [Materiales] SELECT * FROM #TempMateriales;
DROP TABLE #TempMateriales;

-- Inserciones para la tabla Pacientes
INSERT INTO [Pacientes] SELECT * FROM #TempPacientes;
DROP TABLE #TempPacientes;

-- Inserciones para la tabla Pagos
INSERT INTO [Pagos] SELECT * FROM #TempPagos;
DROP TABLE #TempPagos;

-- Inserciones para la tabla Pagos_Cuotas
INSERT INTO [Pagos_Cuotas] SELECT * FROM #TempPagos_Cuotas;
DROP TABLE #TempPagos_Cuotas;

-- Inserciones para la tabla Roles
INSERT INTO [Roles] SELECT * FROM #TempRoles;
DROP TABLE #TempRoles;

-- Inserciones para la tabla Tipo_Cobro
INSERT INTO [Tipo_Cobro] SELECT * FROM #TempTipo_Cobro;
DROP TABLE #TempTipo_Cobro;

-- Inserciones para la tabla Tratamientos
INSERT INTO [Tratamientos] SELECT * FROM #TempTratamientos;
DROP TABLE #TempTratamientos;

-- Inserciones para la tabla Usuarios
INSERT INTO [Usuarios] SELECT * FROM #TempUsuarios;
DROP TABLE #TempUsuarios;

SET FOREIGN_KEY_CHECKS=1;
