# Plan de cambios — Capítulo V: Tactical-Level Software Design

> Documento guía para la reestructuración del Capítulo V antes de generar la nueva versión final del Word.
> **Versión 2 — actualizada con las decisiones confirmadas.**

---

## 0. Decisiones confirmadas (v2)

| Tema | Decisión |
|---|---|
| Idioma del capítulo | **Todo el cuerpo, descripciones y tablas en español.** Nombres de bounded contexts en inglés. Aggregates, eventos, commands, queries, resources, servicios, columnas de BD y demás elementos de código en inglés. |
| BC `Transport Company` | Se renombra a **`Driver`**. |
| `Tariff` | **Es un aggregate independiente**, no Value Object. El conductor define su propio tarifario y además debe llevar un calendario (días que sale, días que no, duración por ruta), por lo que `Tariff` necesita identidad y ciclo de vida propios. |
| `Reservations` | **Queda dentro del BC `Trips`**, como nuevo aggregate. |
| Tipos de vehículo | `Car`, `Pickup`, `Combi`, `Van`, `Bus`, `Minivan`. **Se elimina `Motorcycle`.** |
| Tipo de documento del pasajero | **Solo `DNI`.** |
| Planes de suscripción | **`Free`** y **`Premium`**. |
| Beneficio del plan Premium | **Uso ilimitado del BC `Discovery` con IA.** Free queda con uso limitado. |
| Métodos de pago | `Yape`, `Plin`, `Card`, `Cash`. |
| Diagramas nuevos | Se entregan como **imágenes generadas a partir de PlantUML** en un `.md` aparte. El Word del Capítulo V tendrá los subtítulos correspondientes **sin las imágenes**. |

---

## 1. Resumen ejecutivo del Capítulo V actual

El Capítulo V describe el **diseño táctico** del backend `Frock-backend` usando **Clean Architecture + DDD + CQRS**. Está dividido en 10 bounded contexts, cada uno desarrollado en cuatro capas (Domain, Application, Infrastructure, Interface) más un módulo transversal `Shared`. Para cada BC se documentan: aggregates, entidades, commands, queries, value objects, servicios, repositorios, resources REST, assemblers, controllers, infraestructura, diagramas de componentes (C4 nivel 3) y diagramas de código + diseño de BD.

**Bounded contexts actuales:**

| Nº | Bounded Context | Propósito |
|----|----|----|
| 5.1 | **IAM** | Identidad, autenticación, JWT, perfiles. Contiene `User` y `DriverProfile`. Roles: Traveller, **TransportManager**, Driver, Admin. |
| 5.2 | **Notifications** | Notificaciones por usuario (info/warning/success/error). |
| 5.3 | **Collections** | Colecciones de favoritos del usuario. |
| 5.4 | **Discovery** | Búsqueda y paradas cercanas. |
| 5.5 | **Ratings** | Calificación de conductores tras viajes (referencia `Trip`, `DriverProfile`, `User`). |
| 5.6 | **Routes** | Rutas de transporte (precio, duración, paradas, horarios) integrado con OSRM. Hoy referencia a **Company** (`GetAllRoutesByFkCompanyIdQuery`, `GetRoutesByCompanyIdResource`). |
| 5.7 | **Shared** | Abstracciones transversales (BaseRepository, UoW, dispatcher, Cloudinary, Swagger). |
| 5.8 | **Stops** | Paradas físicas + jerarquía Region → Province → District. Hoy `Stop` referencia **Company** (`FkIdCompany`, `GetAllStopsByFkIdCompanyQuery`, etc.). |
| 5.9 | **Transport Company** | Empresa de transporte: RUC, logo, contacto, dirección, descripción. Único aggregate: `Company`. |
| 5.10 | **Trips** | Viajes realizados por un pasajero con un conductor en una ruta determinada. |

---

## 2. Objetivos de los cambios

1. **Reemplazar `Transport Company` por `Driver` (Conductor) como bounded context**, simplificando el modelo: ya no se gestionan empresas con RUC, sino conductores individuales (personas naturales) con tarifario, calendario y tipo de vehículo.
2. **Agregar funcionalidad de Reservations** al contexto `Trips`, donde los pasajeros pueden reservar plazas validando su DNI.
3. **Crear un nuevo bounded context `Subscriptions`** con dos planes (`Free` / `Premium`), aplicables a pasajeros y conductores; el beneficio del Premium es el uso ilimitado del BC `Discovery` con IA.
4. **Crear un nuevo bounded context `Payments`** que centralice métodos de pago (`Yape`, `Plin`, `Card`, `Cash`) y reembolsos.

---

## 3. Cambio 1 — Renombrar `Transport Company` → `Driver` (sección 5.9)

### 3.1. Justificación
La startup ya no opera con empresas formales (RUC, logo corporativo, dirección física). Cada conductor es una persona natural que define su propio tarifario, su calendario de disponibilidad y el tipo de vehículo que opera. `Company` desaparece como aggregate independiente y `DriverProfile` (hoy dentro de IAM) se convierte en el aggregate principal del nuevo contexto **`Driver`**.

### 3.2. Nueva estructura propuesta para 5.9 (Bounded Context: Driver)

**Descripción del BC (reemplaza la actual):**
> Bounded context responsable de los conductores como personas naturales prestadoras del servicio de transporte. Gestiona el perfil del conductor, su licencia, el vehículo que opera, el tarifario que aplica a sus viajes y el calendario de disponibilidad (días que sale, días que no, y duración estimada por ruta).

#### 3.2.1. Aggregates

| Tipo | Nombre | Descripción | Responsabilidad Principal | Relación con otros elementos |
|---|---|---|---|---|
| Aggregate | **Driver** | Persona natural que ofrece el servicio de transporte | Mantener la integridad de los datos personales del conductor, su licencia y su vehículo | Relacionado con `User` por `FkIdUser`; referenciado por `Stop`, `RouteAggregate`, `Trip`, `Reservation` y `Rating` |
| Aggregate | **Tariff** | Tarifario y calendario operativo del conductor | Definir precios (base, por kilómetro, mínimo) y la disponibilidad por día de la semana, así como la duración estimada por ruta | Pertenece a `Driver` por `FkIdDriver`. Consumido por `Trips` y `Reservations` para cotizar el viaje |
| Value Object | **VehicleType** | Enumerado del tipo de vehículo: `Car`, `Pickup`, `Combi`, `Van`, `Bus`, `Minivan` | Representar el tipo de vehículo permitido por el sistema | Embebido en `Driver` |
| Value Object | **Vehicle** | Datos del vehículo (placa, marca, modelo, año, capacidad, tipo) | Encapsular información del vehículo de forma inmutable | Embebido en `Driver` |
| Value Object | **WeeklyAvailability** | Días de la semana en los que el conductor opera | Modelar el calendario semanal del conductor | Embebido en `Tariff` |
| Value Object | **RouteDuration** | Duración estimada para una ruta específica que opera el conductor | Modelar el tiempo personalizado que el conductor estima para una ruta | Embebido en `Tariff` |

#### 3.2.2. Commands
- `CreateDriverCommand` — crear un conductor asociado a un `User` con rol `Driver`.
- `UpdateDriverCommand` — actualizar datos personales (nombre, teléfono, foto).
- `UpdateVehicleCommand` — actualizar la información del vehículo.
- `CreateTariffCommand` — crear el tarifario con precios y calendario.
- `UpdateTariffCommand` — actualizar precios, calendario o duraciones por ruta.
- `ToggleAvailabilityCommand` — habilitar/deshabilitar disponibilidad diaria (override puntual).
- `DeleteDriverCommand` — baja lógica del conductor.

#### 3.2.3. Queries
- `GetAllDriversQuery`
- `GetDriverByIdQuery`
- `GetDriverByFkIdUserQuery`
- `GetDriversByVehicleTypeQuery` *(filtrar por tipo de vehículo)*
- `GetAvailableDriversByDayOfWeekQuery` *(filtrar por día de la semana operativo)*
- `GetTariffByDriverIdQuery`
- `GetRouteDurationByDriverAndRouteQuery`

#### 3.2.4. Tabla `drivers` (reemplaza la tabla `companies`)

| Nombre | Descripción |
|---|---|
| id | Identificador único del conductor |
| fk_id_user | Clave foránea hacia `users` (rol Driver) |
| first_name | Nombre del conductor |
| last_name | Apellidos del conductor |
| document_number | DNI |
| phone | Teléfono de contacto |
| photo_url | URL de la foto del conductor en Cloudinary |
| license_number | Número de licencia de conducir |
| license_category | Categoría de licencia (A-IIa, A-IIb, A-IIIa, A-IIIb, A-IIIc) |
| vehicle_plate | Placa del vehículo |
| vehicle_brand | Marca del vehículo |
| vehicle_model | Modelo del vehículo |
| vehicle_year | Año del vehículo |
| vehicle_capacity | Capacidad de pasajeros |
| vehicle_type | Tipo de vehículo (`Car` / `Pickup` / `Combi` / `Van` / `Bus` / `Minivan`) |
| is_available | Indica si el conductor está disponible actualmente |
| created_at | Fecha de creación |
| updated_at | Fecha de última actualización |

#### 3.2.5. Tabla `tariffs` (nueva)

| Nombre | Descripción |
|---|---|
| id | Identificador del tarifario |
| fk_id_driver | Clave foránea hacia `drivers` |
| base_fare | Tarifa base al iniciar el viaje |
| price_per_km | Precio por kilómetro recorrido |
| price_per_minute | Precio por minuto de viaje |
| min_fare | Tarifa mínima |
| currency | Moneda (`PEN`) |
| available_days | Días de la semana operativos (almacenados como string CSV o JSON) |
| is_active | Indica si el tarifario está vigente |
| created_at | Fecha de creación |

#### 3.2.6. Tabla `route_durations` (nueva)

| Nombre | Descripción |
|---|---|
| id | Identificador |
| fk_id_tariff | Clave foránea hacia `tariffs` |
| fk_id_route | Clave foránea hacia `routes` |
| estimated_minutes | Duración estimada del conductor para esa ruta |

### 3.3. Impacto en IAM (sección 5.1)
- **Quitar** el aggregate `DriverProfile` de IAM (su responsabilidad se traslada al nuevo BC Driver).
- **Quitar** Commands/Queries/Repository/Resources de `DriverProfile`: `CreateDriverProfileCommand`, `GetDriverProfileByUserIdQuery`, `IDriverProfileRepository`, `DriverProfileRepository`, `CreateDriverProfileResource`, `DriverProfileResource`.
- **Actualizar** el Value Object `Role` para quitar `TransportManager`. Roles finales: `Traveller`, `Driver`, `Admin`.
- **Actualizar** la tabla `users`: quitar el rol `TransportManager` de la descripción.
- **Quitar** la tabla `driver_profiles` del diagrama de BD de IAM.
- **Actualizar** la descripción de 5.1 para reflejar el nuevo alcance (sólo identidad).

### 3.4. Impacto en Routes (sección 5.6)
- Renombrar `GetAllRoutesByFkCompanyIdQuery` → `GetAllRoutesByFkDriverIdQuery`.
- Renombrar `GetRoutesByCompanyIdResource` → `GetRoutesByDriverIdResource`.
- Actualizar la descripción del controlador: el rol autorizado pasa de `TransportManager` a `Driver`. Ej.: *"aplicar AuthorizeAttribute con roles Driver y Admin"*.
- En el aggregate `RouteAggregate` reemplazar `FkIdCompany` por `FkIdDriver` en las relaciones.
- En el diagrama de BD de `routes`: renombrar la columna `fk_id_company` → `fk_id_driver`.

### 3.5. Impacto en Stops (sección 5.8)
- Renombrar `GetAllStopsByFkIdCompanyQuery` → `GetAllStopsByFkIdDriverQuery`.
- Renombrar `GetStopByNameAndFkIdCompanyQuery` → `GetStopByNameAndFkIdDriverQuery`.
- En el aggregate `Stop` actualizar la relación: ya no `Company por FkIdCompany`, sino `Driver por FkIdDriver`.
- En la tabla `stops`: renombrar `fk_id_company` → `fk_id_driver`.

### 3.6. Impacto en Ratings (sección 5.5)
- Reemplazar la referencia a `DriverProfile` por `Driver` en la descripción del aggregate `Rating`.
- La columna `fk_id_driver` en la tabla `ratings` se mantiene (ahora apunta al nuevo aggregate `Driver`).

### 3.7. Impacto en Trips (sección 5.10)
- Reemplazar la referencia a `DriverProfile` por `Driver` en la descripción del aggregate `Trip`.
- La columna `fk_id_driver` en la tabla `trips` se mantiene (ahora apunta al nuevo aggregate `Driver`).

### 3.8. Resumen ejecutivo del cambio 1

| Antes | Después |
|---|---|
| BC: Transport Company | BC: Driver |
| Aggregate `Company` (con RUC, logo, dirección) | Aggregate `Driver` (persona natural con vehículo) + Aggregate `Tariff` (tarifario + calendario) |
| Aggregate `DriverProfile` dentro de IAM | Eliminado (su responsabilidad pasa al BC Driver) |
| Rol `TransportManager` | Eliminado (sólo quedan Traveller / Driver / Admin) |
| `fk_id_company` en Stops y Routes | `fk_id_driver` |

---

## 4. Cambio 2 — Agregar Reservations a Trips (sección 5.10)

### 4.1. Justificación
Hoy `Trip` representa un viaje ya realizado. Se necesita modelar la **intención previa de reservar** una plaza en un viaje futuro. Se introduce como un nuevo aggregate `Reservation` **dentro del BC Trips**, manteniendo la cohesión con el ciclo de vida del viaje.

### 4.2. Nuevos elementos en Trips

#### 4.2.1. Aggregates (añadir)

| Tipo | Nombre | Descripción | Responsabilidad Principal | Relación con otros elementos |
|---|---|---|---|---|
| Aggregate | **Reservation** | Reserva de una plaza realizada por un pasajero para un viaje | Mantener la intención de reserva, validar el DNI del pasajero y gestionar el ciclo de vida de la reserva | Relacionada con `Trip` por `FkIdTrip` y con `User` por `FkIdUser`. Vinculada al BC Payments por `FkIdPayment` |
| Value Object | **DocumentType** | Tipo de documento del pasajero. Único valor permitido en MVP: `DNI` | Representar de forma extensible el tipo de documento aceptado | Embebido en `Reservation` |
| Value Object | **ReservationStatus** | Estado de la reserva (`Pending`, `Confirmed`, `Cancelled`, `Completed`, `Refunded`) | Modelar el ciclo de vida de la reserva | Embebido en `Reservation` |

#### 4.2.2. Commands (añadir)
- `CreateReservationCommand` — crear una reserva con `fk_id_user`, `fk_id_trip`, `document_type` (DNI), `document_number`, `seats`.
- `ConfirmReservationCommand` — confirma la reserva tras un pago exitoso.
- `CancelReservationCommand` — cancela la reserva (puede disparar un `CreateRefundCommand` en el BC Payments).

#### 4.2.3. Queries (añadir)
- `GetReservationByIdQuery`
- `GetReservationsByUserIdQuery`
- `GetReservationsByTripIdQuery`
- `GetReservationsByDriverIdQuery`

#### 4.2.4. Resources / Assemblers / Controller
- Añadir `CreateReservationResource`, `ReservationResource` y sus assemblers.
- Crear un `ReservationsController` separado para mantener el SRP (en lugar de saturar `TripsController`).

#### 4.2.5. Tabla `reservations` (nueva)

| Nombre | Descripción |
|---|---|
| id | Identificador único de la reserva |
| fk_id_user | Clave foránea hacia el pasajero |
| fk_id_trip | Clave foránea hacia el viaje reservado |
| document_type | Tipo de documento (`DNI`) |
| document_number | Número de DNI del pasajero |
| seats | Cantidad de asientos reservados |
| status | Estado (`Pending` / `Confirmed` / `Cancelled` / `Completed` / `Refunded`) |
| fk_id_payment | Clave foránea hacia `payments` (NULL hasta que se pague) |
| reserved_at | Fecha y hora de la reserva |
| confirmed_at | Fecha y hora de confirmación |

### 4.3. Modificación del Aggregate Trip
- Añadir a la descripción del aggregate `Trip` que ahora puede tener **N reservas** asociadas (relación 1:N).
- Añadir columna `available_seats` a la tabla `trips` para llevar control de capacidad disponible.

---

## 5. Cambio 3 — Nuevo Bounded Context: `Subscriptions` (sección 5.11)

### 5.1. Justificación
Tanto pasajeros como conductores tendrán acceso a dos planes: **Free** (limitado) y **Premium** (de pago). El beneficio diferenciador del plan Premium es el **uso ilimitado del BC Discovery con IA**, mientras que Free queda con uso limitado de esa funcionalidad. Es transversal a los roles, por lo que un BC propio es la opción correcta (alta cohesión, separación clara del dominio de identidad y del de pagos).

### 5.2. Estructura propuesta

#### 5.2.1. Aggregates

| Tipo | Nombre | Descripción | Responsabilidad Principal | Relación con otros elementos |
|---|---|---|---|---|
| Aggregate | **Plan** | Plan de suscripción ofrecido por la plataforma | Definir el catálogo de planes (`Free` / `Premium`), su precio, beneficios y duración | Referenciado por `Subscription` |
| Aggregate | **Subscription** | Suscripción activa de un usuario a un plan | Mantener el estado de la suscripción y gestionar su renovación | Relacionada con `User` por `FkIdUser` y con `Plan` por `FkIdPlan`. Vinculada al BC Payments por `FkIdPayment` |
| Value Object | **PlanType** | Enumerado (`Free`, `Premium`) | Identificar el tipo de plan | Embebido en `Plan` |
| Value Object | **SubscriptionStatus** | Estado de la suscripción (`Active`, `Expired`, `Cancelled`, `PendingPayment`) | Modelar el ciclo de vida | Embebido en `Subscription` |
| Value Object | **BillingCycle** | Ciclo de facturación (`Monthly`, `Yearly`) | Definir periodicidad de cobro | Embebido en `Plan` |

#### 5.2.2. Commands
- `CreatePlanCommand` *(admin only)*
- `UpdatePlanCommand` *(admin only)*
- `SubscribeToPlanCommand` — usuario se suscribe a un plan.
- `CancelSubscriptionCommand`
- `RenewSubscriptionCommand`

#### 5.2.3. Queries
- `GetAllPlansQuery`
- `GetPlanByIdQuery`
- `GetActiveSubscriptionByUserIdQuery`
- `GetSubscriptionHistoryByUserIdQuery`

#### 5.2.4. Tabla `plans`

| Nombre | Descripción |
|---|---|
| id | Identificador único |
| name | Nombre del plan (`Free`, `Premium`) |
| plan_type | Tipo (`Free` / `Premium`) |
| target_role | Rol al que aplica (`Traveller` / `Driver` / `Both`) |
| price | Precio del plan |
| currency | Moneda (`PEN`) |
| billing_cycle | `Monthly` / `Yearly` |
| benefits | Descripción de beneficios (para Premium: uso ilimitado del BC Discovery con IA) |
| discovery_quota | Cantidad de consultas a Discovery permitidas por ciclo (NULL = ilimitado en Premium) |
| is_active | Indica si el plan está disponible |

#### 5.2.5. Tabla `subscriptions`

| Nombre | Descripción |
|---|---|
| id | Identificador único |
| fk_id_user | Clave foránea al usuario suscrito |
| fk_id_plan | Clave foránea al plan |
| status | Estado (`Active` / `Expired` / `Cancelled` / `PendingPayment`) |
| starts_at | Fecha de inicio |
| ends_at | Fecha de expiración |
| auto_renew | Indica si se renueva automáticamente |
| fk_id_payment | Clave foránea al último pago realizado |

### 5.3. Impacto en otros contextos
- **IAM:** mantener un método en `IIamContextFacade` para que `Subscriptions` valide la existencia y el rol del usuario.
- **Discovery:** debe consultar al BC Subscriptions (vía `ISubscriptionsContextFacade`) si el usuario tiene un plan activo Premium antes de aceptar consultas ilimitadas con IA. Si está en Free, aplicar el quota definido.
- **Payments:** la suscripción dispara un cobro al BC Payments.

---

## 6. Cambio 4 — Nuevo Bounded Context: `Payments` (sección 5.12)

### 6.1. Justificación
Centralizar pagos en un BC propio respeta los principios DDD: **alta cohesión** (todo lo relacionado a dinero queda junto), **bajo acoplamiento** (Trips/Subscriptions/Reservations no conocen los detalles de Yape/Plin) y **Anti-Corruption Layer** frente a sistemas externos volátiles (igual que ya hace `Routes` con OSRM y `Shared` con Cloudinary). Además permite **reusabilidad** entre Reservations, Subscriptions y futuras features monetizables.

### 6.2. Estructura propuesta

#### 6.2.1. Aggregates

| Tipo | Nombre | Descripción | Responsabilidad Principal | Relación con otros elementos |
|---|---|---|---|---|
| Aggregate | **Payment** | Pago realizado dentro de la plataforma | Registrar el cobro, su estado, método de pago y referencia externa | Relacionado con `User` por `FkIdUser`; consumido por `Reservation` y `Subscription` |
| Aggregate | **Refund** | Reembolso parcial o total asociado a un pago | Gestionar el reembolso de un pago previamente realizado | Relacionado con `Payment` por `FkIdPayment` |
| Value Object | **PaymentMethod** | Método de pago (`Yape`, `Plin`, `Card`, `Cash`) | Identificar el canal de cobro | Embebido en `Payment` |
| Value Object | **PaymentStatus** | Estado (`Pending`, `Completed`, `Failed`, `Refunded`, `PartiallyRefunded`) | Modelar el ciclo de vida del pago | Embebido en `Payment` |
| Value Object | **Money** | Monto (amount + currency) | Representar dinero como objeto inmutable | Embebido en `Payment` y `Refund` |

#### 6.2.2. Commands
- `CreatePaymentCommand` — iniciar un pago.
- `ConfirmPaymentCommand` — webhook de Yape/Plin confirma cobro, o confirmación manual para Card/Cash.
- `FailPaymentCommand` — webhook indica fallo.
- `CreateRefundCommand` — iniciar reembolso.
- `ConfirmRefundCommand`.

#### 6.2.3. Queries
- `GetPaymentByIdQuery`
- `GetPaymentsByUserIdQuery`
- `GetPaymentsByReferenceQuery` (para conciliación con reservas/suscripciones)
- `GetRefundsByPaymentIdQuery`

#### 6.2.4. Servicios externos (Anti-Corruption Layer)
- `IYapePaymentGateway` — adaptador hacia Yape (creación de orden, validación de QR/teléfono, webhook).
- `IPlinPaymentGateway` — adaptador hacia Plin.
- `ICardPaymentGateway` — adaptador hacia pasarela de tarjetas (Culqi, MercadoPago u otra).
- `ICashPaymentHandler` — manejo del flujo de pago en efectivo (registro manual confirmado por el conductor o admin).
- Todos implementan la interfaz común `IPaymentGateway` (patrón Strategy).
- `PaymentGatewayFactory` selecciona el gateway según `PaymentMethod`.

#### 6.2.5. Tabla `payments`

| Nombre | Descripción |
|---|---|
| id | Identificador único del pago |
| fk_id_user | Clave foránea al usuario pagador |
| amount | Monto |
| currency | Moneda (`PEN`) |
| method | Método (`Yape` / `Plin` / `Card` / `Cash`) |
| status | Estado (`Pending` / `Completed` / `Failed` / `Refunded` / `PartiallyRefunded`) |
| external_reference | ID de la transacción en el gateway externo (NULL para `Cash`) |
| reference_type | Tipo de referencia (`Reservation` / `Subscription`) |
| reference_id | ID de la entidad asociada (reserva o suscripción) |
| created_at | Fecha de creación |
| confirmed_at | Fecha de confirmación |

#### 6.2.6. Tabla `refunds`

| Nombre | Descripción |
|---|---|
| id | Identificador del reembolso |
| fk_id_payment | Clave foránea al pago original |
| amount | Monto reembolsado |
| reason | Motivo del reembolso |
| status | Estado del reembolso |
| created_at | Fecha de creación |
| confirmed_at | Fecha de confirmación |

### 6.3. Integración entre contextos
- **Trips/Reservations** → publica evento `ReservationCreated` → el BC Payments crea un `Payment` pendiente y devuelve URL de QR/intent de Yape, o registra la deuda en `Cash`.
- Cuando Yape/Plin/Card notifica vía webhook → `ConfirmPaymentCommand` → `Payments` publica `PaymentConfirmed` → el BC origen marca la reserva/suscripción como confirmada.
- Para `Cash`, el conductor confirma manualmente la cobranza al terminar el viaje.
- Cancelaciones → `Reservations` o `Subscriptions` solicita `CreateRefundCommand` al BC Payments.

---

## 7. Cambios en diagramas

> **Decisión confirmada:** todos los diagramas nuevos se generan a partir de **PlantUML** en un archivo `.md` separado (`DIAGRAMAS_CAPITULO_V.md`). El Word del Capítulo V incluirá los subtítulos sin las imágenes (las imágenes se insertarán manualmente después).

### 7.1. Diagramas que deben **eliminarse**
- Diagrama de componentes y de clases del antiguo BC Transport Company (sección 5.9 actual).
- Diagrama de BD de la tabla `companies`.
- Diagrama de BD de la tabla `driver_profiles` dentro de IAM (5.1.6.2).

### 7.2. Diagramas que deben **modificarse**
- **IAM (5.1.6.2 — Database Design Diagram):** quitar columnas heredadas de `driver_profiles`. Quitar `TransportManager` del enum `role`.
- **IAM (5.1.6.1 — Class Diagram):** quitar `DriverProfile` del diagrama de clases.
- **Routes (5.6.6.2):** renombrar `fk_id_company` → `fk_id_driver` en `routes`.
- **Stops (5.8.6.2):** renombrar `fk_id_company` → `fk_id_driver` en `stops`.
- **Trips (5.10.6.1 y 5.10.6.2):** añadir `Reservation` al diagrama de clases; añadir tabla `reservations` al diseño de BD.
- **Diagrama Global de Bounded Contexts (final del capítulo):** quitar BC Transport Company; añadir BC Driver, Subscriptions y Payments; actualizar las flechas de relación entre contextos.

### 7.3. Diagramas **nuevos** a generar en PlantUML (en `.md` separado)
1. **Sección 5.9 (Driver):**
   - Class Diagram del Domain Layer (Driver, Tariff, Vehicle, VehicleType, WeeklyAvailability, RouteDuration).
   - Component Diagram nivel C4 del container `Driver Application`.
   - ER Diagram de las tablas `drivers`, `tariffs` y `route_durations`.
2. **Sección 5.10 (Trips ampliado):**
   - Class Diagram actualizado con `Reservation`, `DocumentType`, `ReservationStatus`.
   - ER Diagram actualizado con `reservations` y la nueva columna `available_seats` en `trips`.
3. **Sección 5.11 (Subscriptions — nueva):**
   - Class Diagram de `Plan`, `Subscription`, `PlanType`, `SubscriptionStatus`, `BillingCycle`.
   - Component Diagram nivel C4 del container `Subscriptions Application`.
   - ER Diagram de `plans` y `subscriptions`.
4. **Sección 5.12 (Payments — nueva):**
   - Class Diagram de `Payment`, `Refund`, `PaymentMethod`, `PaymentStatus`, `Money`, y los gateways (`IPaymentGateway`, `IYapePaymentGateway`, `IPlinPaymentGateway`, `ICardPaymentGateway`, `ICashPaymentHandler`).
   - Component Diagram nivel C4 del container `Payments Application` mostrando los gateways Yape/Plin/Card como sistemas externos.
   - ER Diagram de `payments` y `refunds`.
5. **Diagrama Global de relaciones entre BCs** actualizado, mostrando las dependencias entre Trips ↔ Payments ↔ Subscriptions ↔ Driver ↔ IAM ↔ Discovery, etc.

---

## 8. Estructura final del Capítulo V tras los cambios

| Nº | Bounded Context | Estado |
|---|---|---|
| 5.1 | IAM | **Modificado** (quitar DriverProfile, quitar rol TransportManager) |
| 5.2 | Notifications | Sin cambios |
| 5.3 | Collections | Sin cambios |
| 5.4 | Discovery | **Modificación menor** (consultar plan activo vía Subscriptions Facade) |
| 5.5 | Ratings | **Modificación menor** (referencia `DriverProfile` → `Driver`) |
| 5.6 | Routes | **Modificado** (`Company` → `Driver` en queries, resources y BD) |
| 5.7 | Shared | Sin cambios |
| 5.8 | Stops | **Modificado** (`fk_id_company` → `fk_id_driver`) |
| 5.9 | ~~Transport Company~~ → **Driver** | **Reemplazado completamente** |
| 5.10 | Trips | **Ampliado** con Reservations |
| **5.11** | **Subscriptions** | **Nuevo** |
| **5.12** | **Payments** | **Nuevo** |

---

## 9. Entregables a generar tras la confirmación

1. **`DIAGRAMAS_CAPITULO_V.md`** — archivo con todos los bloques PlantUML listos para renderizar como imágenes (diagramas de clases, de componentes C4 nivel 3, ER y diagrama global de bounded contexts).
2. **`Capitulo_V_Final.docx`** — Word del Capítulo V completo en español, con:
   - Nombres de bounded contexts en inglés.
   - Aggregates, commands, queries, events, columnas de BD y demás elementos de código en inglés.
   - Todas las descripciones, encabezados de tabla y texto explicativo en español.
   - Subtítulos de las secciones de diagramas presentes, **sin imágenes embebidas** (espacios listos para insertarlas manualmente).

> **Confirma este plan v2 y procedo a generar ambos entregables.**
