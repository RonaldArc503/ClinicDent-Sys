-- Exportación SQL de ClinicaDental
-- Fecha: 2025-05-25 23:34:00
SET IDENTITY_INSERT ON;
BEGIN TRANSACTION;

-- Datos de la tabla Citas
DELETE FROM [Citas]; -- Elimina datos existentes para reemplazar

INSERT INTO [Citas] ([id_cita], [id_paciente], [id_dentista], [fecha_hora], [estado]) VALUES (1, 1, 1, '26/5/2025 13:20:00', 'Pendiente');

-- Datos de la tabla Consulta
DELETE FROM [Consulta]; -- Elimina datos existentes para reemplazar

INSERT INTO [Consulta] ([id_consulta], [id_cita], [id_dentista], [id_paciente], [fecha_consulta], [diagnostico], [observaciones], [recomendaciones], [requiere_tratamiento]) VALUES (1, 1, 1, 1, '26/5/2025 13:20:00', NULL, NULL, NULL, 1);

-- Datos de la tabla Consultas_Tratamientos
DELETE FROM [Consultas_Tratamientos]; -- Elimina datos existentes para reemplazar

INSERT INTO [Consultas_Tratamientos] ([id_consulta_tratamiento], [id_consulta], [id_tratamiento], [cantidad], [total]) VALUES (1, 1, 1, 1, 40.00);

-- Datos de la tabla Dentistas
DELETE FROM [Dentistas]; -- Elimina datos existentes para reemplazar

INSERT INTO [Dentistas] ([id_dentista], [nombre], [apellido], [telefono], [correo], [especialidad], [activo]) VALUES (1, 'Hoal ', 'magana', '5454-5757', 'erer@g.com', 'Odontologia', 1);

-- Datos de la tabla Historial_Clinico
DELETE FROM [Historial_Clinico]; -- Elimina datos existentes para reemplazar

INSERT INTO [Historial_Clinico] ([id_historial], [id_consulta_tratamiento], [fecha_registro], [tipo]) VALUES (1, NULL, '25/5/2025 00:00:00', 'Consulta');
INSERT INTO [Historial_Clinico] ([id_historial], [id_consulta_tratamiento], [fecha_registro], [tipo]) VALUES (2, 1, '25/5/2025 00:00:00', 'Tratamiento');

-- Datos de la tabla Historial_Pagos
DELETE FROM [Historial_Pagos]; -- Elimina datos existentes para reemplazar

INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (4, NULL, 2, 'Completado');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (5, 4, 3, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (6, 5, 3, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (7, 6, 4, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (8, 7, 4, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (9, 8, 4, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (10, 9, 4, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (11, 10, 4, 'Pendiente');
INSERT INTO [Historial_Pagos] ([id_historial_pago], [id_cuota], [id_pago], [estado]) VALUES (12, 11, 4, 'Pendiente');

-- Datos de la tabla Inventario
DELETE FROM [Inventario]; -- Elimina datos existentes para reemplazar

INSERT INTO [Inventario] ([id_inventario], [id_material], [cantidad], [fecha_actualizacion]) VALUES (1, 1, 2, '19/5/2025 00:00:00');
INSERT INTO [Inventario] ([id_inventario], [id_material], [cantidad], [fecha_actualizacion]) VALUES (2, 1, 2, '19/5/2025 00:00:00');
INSERT INTO [Inventario] ([id_inventario], [id_material], [cantidad], [fecha_actualizacion]) VALUES (3, 1, 2, '19/5/2025 00:00:00');

-- Datos de la tabla Materiales
DELETE FROM [Materiales]; -- Elimina datos existentes para reemplazar

INSERT INTO [Materiales] ([id_material], [nombre], [cantidad], [proveedor], [fecha_caducidad], [minimo_stock], [descripcion], [estado]) VALUES (1, 'Anestecia', 23, 'Claro', '1/5/2029 00:00:00', 23, 'dsffds', '1');
INSERT INTO [Materiales] ([id_material], [nombre], [cantidad], [proveedor], [fecha_caducidad], [minimo_stock], [descripcion], [estado]) VALUES (7, 'Anestecia', 45, 'Claro', '17/6/2025 00:00:00', 10, '424242', '1');

-- Datos de la tabla Pacientes
DELETE FROM [Pacientes]; -- Elimina datos existentes para reemplazar

INSERT INTO [Pacientes] ([id_paciente], [nombres], [apellidos], [fecha_nacimiento], [telefono], [genero], [alergias], [activo]) VALUES (1, 'Ronald Alexander', 'Ordones Lopez', '10/4/1980 00:00:00', '5505-0505', 'Masculino', NULL, 1);
INSERT INTO [Pacientes] ([id_paciente], [nombres], [apellidos], [fecha_nacimiento], [telefono], [genero], [alergias], [activo]) VALUES (2, 'Ronald Aleander', 'Ordones Lopez', '9/5/1980 00:00:00', '4044-0404', 'Masculino', NULL, 1);

-- Datos de la tabla Pagos
DELETE FROM [Pagos]; -- Elimina datos existentes para reemplazar

INSERT INTO [Pagos] ([id_pago], [id_consulta], [fecha_pago], [monto_total], [metodo_pago], [tipo_pago]) VALUES (2, 1, '25/5/2025 12:44:27', 25.00, 'Efectivo', 'Unico');
INSERT INTO [Pagos] ([id_pago], [id_consulta], [fecha_pago], [monto_total], [metodo_pago], [tipo_pago]) VALUES (3, 1, '25/5/2025 12:45:10', 25.00, 'Efectivo', 'Cuotas');
INSERT INTO [Pagos] ([id_pago], [id_consulta], [fecha_pago], [monto_total], [metodo_pago], [tipo_pago]) VALUES (4, 1, '25/5/2025 12:49:20', 40.00, 'Efectivo', 'Cuotas');

-- Datos de la tabla Pagos_Cuotas
DELETE FROM [Pagos_Cuotas]; -- Elimina datos existentes para reemplazar

INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (4, 3, '25/5/2025 00:00:00', 13.00, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (5, 3, '25/6/2025 00:00:00', 12.00, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (6, 4, '25/5/2025 00:00:00', 6.67, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (7, 4, '25/6/2025 00:00:00', 6.67, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (8, 4, '25/7/2025 00:00:00', 6.67, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (9, 4, '25/8/2025 00:00:00', 6.67, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (10, 4, '25/9/2025 00:00:00', 6.67, 'Pendiente');
INSERT INTO [Pagos_Cuotas] ([id_cuota], [id_pago], [fecha_pago], [monto], [estado]) VALUES (11, 4, '25/10/2025 00:00:00', 6.65, 'Pendiente');

-- Datos de la tabla Roles
DELETE FROM [Roles]; -- Elimina datos existentes para reemplazar

INSERT INTO [Roles] ([id_rol], [nombre]) VALUES (1, 'Administrador');
INSERT INTO [Roles] ([id_rol], [nombre]) VALUES (3, 'Usuario');

-- Datos de la tabla Tipo_Cobro
DELETE FROM [Tipo_Cobro]; -- Elimina datos existentes para reemplazar

INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (2, 'Blanqueamiento dental');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (9, 'Cancer');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (8, 'Carillas dentales');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (5, 'Endodoncia');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (6, 'Extracción dental');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (4, 'Implante dental');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (1, 'Limpieza dental');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (3, 'Ortodoncia');
INSERT INTO [Tipo_Cobro] ([id_tipo_cobro], [nombre]) VALUES (7, 'Prótesis dental');

-- Datos de la tabla Tratamientos
DELETE FROM [Tratamientos]; -- Elimina datos existentes para reemplazar

INSERT INTO [Tratamientos] ([id_tratamiento], [id_tipo_cobro], [fecha_inicio], [costo], [duracion_estimada], [seguimiento], [dientes_seleccionados]) VALUES (1, 6, '25/5/2025 00:00:00', 40.00, 7, 1, '{"17":{"partes":{"superior":"caries","inferior":"caries","derecha":"caries","izquierda":"caries"},"marca":null},"puentes":[]}');

-- Datos de la tabla Usuarios
DELETE FROM [Usuarios]; -- Elimina datos existentes para reemplazar

INSERT INTO [Usuarios] ([id_usuario], [nombre], [apellido], [correo], [usuario], [clave], [id_rol]) VALUES (5, 'Usuario', 'Usuario', 'root@root.com', 'usuario', '$2a$11$xD3IPjVatfi3xWXeC3IwKOfs9tJXQdf.o8jn1vj8RFOUAN6yeXwJe', 1);

COMMIT TRANSACTION;
SET IDENTITY_INSERT OFF;
