|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Sistemas Informáticos de Facturación**   * **Remisión voluntaria** * **Remisión bajo requerimiento de la AEAT** | | | | |
| **Autor:** AEAT | **Fecha:** | 28/07/2025 | **Versión:** | 1.0.3 |

**Revisiones**

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
| **Edic.** | **Rev.** | **Fecha** | **Descripción** | **A(\*)** | **Páginas** |
| 0 | 0.1.0 | 26/07/2023 | Versión inicial **BORRADOR** |  | 28 |
| 0 | 0.2.0 | 21/12/2023 | Modificación de esquemas y definición operaciones |  | 55 |
| 0 | 0.3.0 | 20/05/2024 | Modificación de esquemas y definición operaciones |  | 61 |
| 0 | 0.3.1 | 29/05/2024 | Tratamiento de cadenas de texto, punto 6.6 | A | 62 |
| 0 | 0.3.2 | 04/07/2024 | Aclaraciones en descripción operativa | R | 62 |
| 0 | 0.4.0 | 25/09/2024 | Actualizacion de los esquemas XSD | R | 62 |
| 0 | 0.4.1 | 18/10/2024 | Actualizacion de el esquema de respuesta XSD | R | 62 |
| 0 | 0.4.2 | 28/10/2024 | Actualizacion de los esquemas XSD | R | 62 |
| 0 | 1.0.0 | 20/02/2025 | Remisión bajo requerimiento Consulta de información presentada  Matizaciones operativa Requerimiento  Nombre emisor en consulta por destinatario (opcional)  Mostrar Sistema Informático de Facturación (opcional)  Consulta ampliada por identificador RefExterna | R | 96 |
| 0 | 1.0.1 | 23/04/2025 | Publicación de URLs de producción | R | 100 |
|  | 1.0.2 | 16/07/2025 | Cambio en consulta de SIF | R | 100 |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
|  | 1.0.3 | 28/07/2025 | Errata en tamaño campo IdPeticionRegistroDuplicado | R | 100 |

**(\*) Acción: A = Añadir; R = Reemplazar**

**Índice**

1. [**INTRODUCCIÓN. 7**](#_bookmark0)
2. [**CONTROL DE VERSIONES. 8**](#_bookmark1)

|  |  |  |
| --- | --- | --- |
| [**2.1.**](#_bookmark2) | [**Versión 0.1.0**](#_bookmark2) | [**8**](#_bookmark2) |
| [**2.1.**](#_bookmark3) | [**Versión 0.2.0**](#_bookmark3) | [**8**](#_bookmark3) |
| [**2.1.**](#_bookmark4) | [**Versión 0.3.0**](#_bookmark4) | [**8**](#_bookmark4) |
| [**2.1.**](#_bookmark5) | [**Versión 0.4.0**](#_bookmark5) | [**8**](#_bookmark5) |
| [**2.1.**](#_bookmark6) | [**Versión 0.4.1**](#_bookmark6) | [**9**](#_bookmark6) |
| [**2.1.**](#_bookmark7) | [**Versión 0.4.2**](#_bookmark7) | [**9**](#_bookmark7) |
| [**2.1.**](#_bookmark8) | [**Versión 1.0.0**](#_bookmark8) | [**9**](#_bookmark8) |

1. [**ESQUEMA GENERAL DE FUNCIONAMIENTO. 9**](#_bookmark9)

[Descripción de los valores posibles de la respuesta global (o “estados globales”) de una remisión. 11](#_bookmark10) [Tipos de Errores definidos. 12](#_bookmark11)

[Tratamiento de los errores (no admisibles y admisibles). 12](#_bookmark12)

1. [ESTÁNDARES Y REQUISITOS. 13](#_bookmark13)
   1. [Introducción. 13](#_bookmark14)
   2. [Estándares utilizados. 13](#_bookmark15)
   3. [Medio de envío. 14](#_bookmark16)
2. [CONSIDERACIONES DE DISEÑO. 14](#_bookmark17)
   1. [Comunicación de incidencias en el procesado de peticiones. 14](#_bookmark18)
3. [DISEÑO. 16](#_bookmark19)
   1. [Estructura de los mensajes. 16](#_bookmark20)
   2. [Tipos de mensajes. 16](#_bookmark21)
      1. [Definición del mensaje de remisión. 16](#_bookmark22)
      2. [Alta/Anulación de registros de facturación. 17](#_bookmark23)
      3. [Consulta de registros de facturación presentados (servicio solo disponible en](#_bookmark24) [remisión voluntaria «VERI\*FACTU»). 22](#_bookmark24)
      4. [Definición de los mensajes de respuesta. 24](#_bookmark25)
      5. [Respuesta Alta/Anulación de registros de facturación. 24](#_bookmark26)
      6. [Respuesta de la Consulta de registros de facturación presentados. 26](#_bookmark27)
   3. [Especificación funcional del mensaje de remisión. 29](#_bookmark28)
   4. [Especificación funcional de la consulta, por emisor y destinatario, de los registos de](#_bookmark29) [facturación presentados (servicio solo disponible en remisión voluntaria «VERI\*FACTU»). 30](#_bookmark29)
      1. [Consulta de los registos de facturación presentados 30](#_bookmark30)
      2. [Respuesta de la consulta de los registos de facturación presentados 34](#_bookmark31)
      3. [Consulta paginada 36](#_bookmark32)
      4. [Respuesta de alta y anulación de registros de facturación. 37](#_bookmark33)
         1. [Mecanismo de control de flujo. 40](#_bookmark34)
   5. [Valores permitidos en campos de tipo lista. 41](#_bookmark35)
      1. [Alta, anulación y consulta de registros de facturación. 41](#_bookmark36)
      2. [Consulta de registros de facturación. 41](#_bookmark37)
      3. [Respuesta de alta y anulación de registros de facturación. 42](#_bookmark38)
   6. [Remisión voluntaria y bajo requerimiento. 44](#_bookmark39)
   7. [Tratamiento de cadenas de texto en campos XML 46](#_bookmark40)
   8. [Valores permitidos en campos numéricos. 46](#_bookmark41)
   9. [Aclaración sobre escapado de caracteres especiales. 47](#_bookmark42)
4. [ANEXO I: DEFINICIÓN DE SERVICIOS Y ESQUEMAS (ENTORNO DE](#_bookmark43) [PRUEBAS). 48](#_bookmark43)
   1. [Definición de servicios. 48](#_bookmark44)
   2. [Esquemas de Entrada 48](#_bookmark45)
   3. [Esquemas de Salida. 49](#_bookmark46)
5. [ANEXO I: DEFINICIÓN DE SERVICIOS Y ESQUEMAS (ENTORNO DE](#_bookmark47) [PRODUCCIÓN). 50](#_bookmark47)
   1. [Definición de servicios. 50](#_bookmark48)
   2. [Esquemas de Entrada 50](#_bookmark49)
   3. [Esquemas de Salida. 51](#_bookmark50)
6. [ANEXO II: OPERATIVA DE REMISIÓN VOLUNTARIA «VERI\*FACTU» 52](#_bookmark51)
   1. [Operativa: Alta de un registro de facturación. 52](#_bookmark52)
      1. [Alta inicial (“normal”) del registro de facturación. 52](#_bookmark53)
         1. [Ejemplo mensaje XML de alta inicial (“normal”) del registro de facturación. 53](#_bookmark54)
      2. [Subsanación del registro de facturación. 56](#_bookmark55)
         1. [Ejemplo mensaje XML de subsanación del registro de facturación. 57](#_bookmark56)
      3. [Alta, tras un rechazo previo del registro de facturación de alta inicial (y que, por tanto, no](#_bookmark57) [existe aún en la AEAT). 60](#_bookmark57)
         1. [Ejemplo mensaje XML de alta, tras un rechazo previo del registro de facturación de alta](#_bookmark58) [inicial (y que, por tanto, no existe aún en la AEAT). 61](#_bookmark58)
   2. [Operativa: Anulación de un registro de facturación. 64](#_bookmark59)
      1. [Anulación del registro de facturación. 64](#_bookmark60)
         1. [Ejemplo mensaje XML de anulación del registro de facturación. 65](#_bookmark61)
      2. [Anulación, tras un rechazo previo del registro de facturación de anulación. 67](#_bookmark62)
         1. [Ejemplo mensaje XML de anulación, tras un rechazo previo del registro de facturación](#_bookmark63) [de anulación. 68](#_bookmark63)
      3. [Anulación cuando el registro de facturación que se quiere anular NO está registrado en la](#_bookmark64) [AEAT. 71](#_bookmark64)
         1. [Ejemplo mensaje XML de anulación cuando el registro de facturación que se quiere](#_bookmark65) [anular NO está registrado en la AEAT. 72](#_bookmark65)
   3. [Operativa habitual de remisión agrupada de registros de facturación. 74](#_bookmark66)
      1. [Ejemplo de mensaje XML que incluye tres registros de facturación (dos registros de](#_bookmark67) [facturación de alta y uno de anulación). 74](#_bookmark67)
7. [ANEXO III: OPERATIVA DE REMISIÓN DE REGISTROS DE FACTURACIÓN](#_bookmark68) [PARA RESPONDER A UN REQUERIMIENTO DE LA AEAT «NO VERI\*FACTU». 80](#_bookmark68)
   1. [Operativa: Alta de un registro de facturación. 81](#_bookmark69)
      1. [Alta inicial (“normal”) del registro de facturación. 81](#_bookmark70)
         1. [Ejemplo mensaje XML de alta inicial (“normal”) del registro de facturación. 82](#_bookmark71)
      2. [Subsanación del registro de facturación. 85](#_bookmark72)
         1. [Ejemplo mensaje XML de subsanación del registro de facturación. 86](#_bookmark73)
   2. [Operativa: Anulación de un registro de facturación. 89](#_bookmark74)
      1. [Anulación del registro de facturación. 89](#_bookmark75)
         1. [Ejemplo mensaje XML de anulación del registro de facturación. 90](#_bookmark76)
8. [ANEXO IV: OPERATIVA DE CONSULTA DE INFORMACIÓN PRESENTADA](#_bookmark77) [(SERVICIO SOLO DISPONIBLE EN REMISIÓN VOLUNTARIA «VERI\*FACTU») 92](#_bookmark77)
   1. [Operativa: Consulta del emisor de los registros de facturación para obtener los registros](#_bookmark78) [presentados. 93](#_bookmark78)
      1. [Consulta de registros de facturación presentados previamente ordenados por fecha de](#_bookmark79) [presentación 93](#_bookmark79)
         1. [Ejemplo mensaje XML de consulta de registros de facturación presentados](#_bookmark80) [previamente. Consulta del emisor del registro de facturación. 93](#_bookmark80)
         2. [Ejemplo mensaje XML de consulta de registros de facturación presentados](#_bookmark81) [previamente filtrando por ejercicio, periodo y NIF de la contraparte. Consulta del emisor del](#_bookmark81) [registro de facturación. 97](#_bookmark81)
         3. [Ejemplo mensaje XML de consulta de registros de facturación presentados](#_bookmark82) [previamente filtrando por ejercicio, periodo y un rago de fecha de expedición. Consulta del emisor](#_bookmark82) [del registro de facturación. 98](#_bookmark82)
      2. [Consulta del destinatario (cliente) de los registros de facturación para obtener los registros](#_bookmark83) [presentados por su proveedor. 99](#_bookmark83)
         1. [Ejemplo mensaje XML de consulta paginada de registros de facturación presentados](#_bookmark84) [previamente. Consulta del destinatario del registro de facturación. 100](#_bookmark84)

# Introducción.

El reglamento aprobado por el Real Decreto 1007/2023, de 5 de diciembre, establece en su artículo 15 que los obligados tributarios que utilicen sistemas informáticos para el cumplimiento de la obligación de facturación podrán remitir voluntariamente a la Agencia Estatal de Administración Tributaria (AEAT o Agencia Tributaria), de forma automática y segura por medios electrónicos, todos los registros de facturación generados por esos sistemas informáticos de acuerdo con unas especificaciones técnicas que se recogen en la Orden Ministerial HAC7/1177/2024, por la que se regulan los Sistemas Informáticos de Facturación y VERI\*FACTU. Esta orden, en su disposición adicional única, habilita a la AEAT para que publique en su sede electrónica cuantos detalles sean necesarios para completar dichas especificaciones.

Este documento detalla los requisitos técnicos necesarios para la remisión de los registros de facturación generados por los sistemas informáticos a la Agencia Estatal de Administración Tributaria.

Para el desarrollo del proyecto se ha considerado importante definirlo bajo estándares que faciliten su desarrollo y que permitan una alta funcionalidad, para ello se utilizarán servicios web que permitirán un envío de los registros de facturación en tiempo real.

Este documento recoge tanto los servicios web destinados al envío voluntario de registros de facturación (sistemas que emiten facturas verificables) como a la remisión de registros de facturación para responder a un requerimiento.

Existe un único formato de registro de facturación para remisión voluntaria por sistemas y para contestar a un requerimiento.

# Control de versiones.

## *Versión 0.1.0*

Creación del documento.

## *Versión 0.2.0*

Modificación de esquemas de entrada. Definición de la operativa «VERI\*FACTU».

## *Versión 0.3.0*

Modificación de esquemas de entrada. Modificación de la operativa «VERI\*FACTU».

Unificación de esquemas para remisión voluntaria y por requerimiento.

Posibilidad de remisión de registros de **alta** y de **anulación** dentro de un mismo envío.

## *Versión 0.4.0*

Modificación del esquema de respuesta para añadir información de la factura registrada previamente en el sistema en el caso de que se rechace el registro por duplicado

Se añade el campo <Representante> en la cabecera para los casos de Colaboración social transitiva, donde existe un obligado, que tiene un asesor fiscal (representante) que a su vez utiliza una plataforma en la nube (que utiliza su propio certificado para la remisión). De esta forma queda constancia de la relación obligado-asesor, si no, sólo quedaría la de obligado-plataforma tecnológica

Sólo cuando el motivo de la remisión sea para dar respuesta a un requerimiento. Se añade el campo opcional <FinRequerimiento> en la cabecera para especificar que se ha finalizado la remisión de registros de facturación tras un requerimiento.

Se añade el campo <Impuesto> para contemplar cuatro tipos de impuestos: IVA, IPSI, IGIC y Otros. Esto implicará que "IPSI" y "Otros" no tengan una serie de validaciones extendidas

## *Versión 0.4.1*

Modificación del esquema de respuesta añadiendo el nodo <Operacion> con información del tipo de operación realizada en el registro de facturación.

## *Versión 0.4.2*

Modificación del namespace de las etiquetas <Cabecera>, <RegistroAlta> y

<RegistroAnulacion>

Modificación del esquema de respuesta en el formato del campo

<TimestampPresentacion> añadiendo el huso horario de los servidores de la AEAT.

## *Versión 1.0.0*

Remisión de registros de facturación bajo requerimiento de la AEAT.

Consulta de la información presentada por el emisor de la factura. La consulta la puede realizar tanto el emisor del registro de facturación como el destinatario (servicio solo disponible en remisión voluntaria «VERI\*FACTU»).

Se modifica la explicación de la operativa de las operaciones de alta por subsanación y anulación en remisión por requerimiento para un mejor entendimiento.

Se añade la posibilidad de consultar el nombre del emisor cuando se consulta por destinatario, aunque puede empeorar el rendimiento de la consulta.

Se añade la posibilidad de obtener en la respuesta el Sistema Informático de Facturación (SIF) del registro de facturación, únicamente cuando se realiza la consulta por obligado a emisión. Esta posibilidad puede empeorar el rendimiento de la consulta.

Se añade la posibilidad de filtrar por el campo “Referencia externa” (“RefExterna”) en las consultas de registro informático.

## *Versión 1.0.2*

Se añade la posibilidad de consultar por Sistema Informático de Facturación (SIF) sin necesidad de cumplimentar todos los campos que identifican a un SIF.

# Esquema general de funcionamiento.

Los sistemas realizarán la remisión de registros de facturación por vía telemática, concretamente mediante Servicios Web SOAP basados en el intercambio de mensajes

XML. Todos los mensajes enviados se responden de forma síncrona y los sistemas deberán procesar adecuadamente los mensajes de respuesta a los envíos realizados.

Se define un solo tipo de mensaje que puede contener registros de facturación tanto de alta como de anulación. Una vez enviado el mensaje XML, la AEAT procederá a realizar automáticamente un proceso de validación, tanto a nivel de formato XML, como de reglas de negocio.

Si el mensaje no supera alguna de las validaciones a nivel de formato XML, se devolverá un mensaje de tipo «SoapFault», en el que se especificará el error concreto.

Si el mensaje supera las validaciones a nivel de formato XML, se procederá a realizar las validaciones de negocio, devolviéndose un mensaje de respuesta con el resultado de la validación y de su aceptación o no por la AEAT.

El número máximo de registros por envío es de 1.000.

La unidad de información, representada por un registro de facturación, podrá ser aceptada, rechazada o aceptada con errores, consecuencia de las validaciones que se realizan en el momento de la remisión.

Si los registros contuvieran errores, sólo se aceptarán aquellos registros para los que no exista motivo de rechazo. En caso de rechazo, los obligados tributarios deberán realizar las subsanaciones necesarias y proceder a una nueva remisión en la que incluirán los registros que en su momento fueron rechazados.

El mensaje XML de respuesta enviado por la AEAT contendrá la relación de registros aceptados y rechazados junto con la expresión del motivo por el que hayan sido rechazados. En la respuesta también se informará del código seguro de verificación (CSV) que servirá para dejar constancia de la remisión, excepto en el caso de que se rechacen todos los registros enviados.

A su vez, en la respuesta también se incluye un resultado global de la remisión, que puede ser aceptada (si no existen errores en ningún registro), aceptada parcialmente (cuando existen registros aceptados y rechazados, o cuando existen registros aceptados con errores admisibles) y rechazada (cuando todos los registros han sido rechazados).

La etiqueta *<IDFactura>* contiene los campos que identifican de manera única a un registro de facturación que son:

*<NIF> + <NumSerieFactura> + <FechaExpedicionFactura>*.

El formato del mensaje XML es el mismo tanto para una remisión voluntaria como para la remisión a raíz de un requerimiento de la AEAT. Tener un esquema XSD único de

validación, facilitará la migración de sistemas que inicialmente emiten facturas no verificables, a sistemas que emiten facturas verificables.

Aunque el esquema XSD es común, existirán URLs diferentes para la remisión voluntaria y la remisión por requerimiento, pudiendo tener los servicios “ubicados” en dichas URLs ciertas diferencias en las validaciones de negocio.

Los envíos a través de los servicios web devolverán un mensaje de respuesta en el que se indicará tanto el resultado de la validación global del envío, como el resultado específico de la validación de cada registro.

Para calificar el resultado global del envío se devolverá uno de los siguientes valores como respuesta:

* Correcto (Aceptación completa)
* ParcialmenteCorrecto (Aceptación parcial)
* Incorrecto (Rechazo completo)

Para calificar el resultado de cada registro de facturación incluido en la remisión se podrá devolver uno de los siguientes valores como respuesta:

* Correcto (Aceptado)
* AceptadoConErrores (Aceptado con errores)
* Incorrecto (Rechazado)

##### Descripción de los valores posibles de la respuesta global (o “estados globales”) de una remisión.

*Aceptación completa*

Una remisión cuyo resultado sea la aceptación completa de la misma, indicará que todos los registros incluidos en la misma han pasado tanto las validaciones sintácticas, como las de negocio y que por tanto han sido registradas de manera satisfactoria por la Agencia.

*Aceptación Parcial*

Una remisión con aceptación parcial indicará que en ella existen registros aceptados y rechazados o existen registros aceptados con errores admisibles, es decir, no todos los registros han sido aceptados correctamente y que, por tanto, los no aceptados -es decir, los rechazados- o bien los aceptados con errores admisibles no han pasado algún tipo de validación de las establecidas.

En el caso de la remisión voluntaria, será necesario efectuar una nueva remisión (previa subsanación, si procede, de los registros de facturación necesarios) que permita la aceptación de todos los registros erróneos.

En el caso de la remisión bajo requerimiento de la AEAT, no debe subsanar los errores relacionados con las validaciones de negocio de los registros enviados, ya que estos registros deben ser los que se han conservado en el sistema del obligado tributario en el momento de su generación.

*Rechazo completo*

Una remisión con un rechazo completo de la misma puede deberse a dos casuísticas:

1. O bien la estructura definida en la remisión no es conforme al esquema definido (no cumple las validaciones estructurales), o bien, existen errores sintácticos en la cabecera y por ello toda la remisión ha de ser rechazada.

La respuesta se devolverá un mensaje de tipo «SoapFault», en el que se especifica el error concreto.

1. Todos los registros incluidos en la remisión incumplen las validaciones sintácticas o de negocio asociadas y por tanto todos ellos han sido rechazados. En este caso se devuelven los códigos de error por los que han sido rechadados cada uno de los registros.

##### Tipos de Errores definidos.

Errores “No admisibles”: Estos errores provocan el rechazo del registro de facturación y son aquellos errores que en ningún caso podrán ser admitidos por la Agencia Tributaria. Se corresponden con los errores provocados al no superar las validaciones sintácticas del envío y a errores de negocio (según se definen en el documento de validaciones).

Errores “Admisibles”: Son aquellos errores que no provocan el rechazo del registro de facturación. Estos registros serán admitidos por la Agencia Tributaria. Y la respuesta dada para este tipo de errores será especificada como “Aceptado con errores”, para dejar constancia al presentador de que se ha producido un error, pero no ha impedido su registro por la Agencia Tributaria.

##### Tratamiento de los errores (no admisibles y admisibles).

En el caso de la remisión voluntaria, tanto los registros de facturación con errores no admisibles que fueron rechazados (y, por lo tanto, no registrados por los sistemas de

la Agencia Tributaria), como los registros de facturación con errores admisibles que sí fueron registrados por los sistemas de la Agencia Tributaria, siempre y cuando para arreglar esa situación no proceda la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de Facturación) ni se anule la factura, deberán ser subsanados y remitidos de nuevo a la AEAT para poder llevar a cabo el tratamiento y validación de los mismos.

En el caso de la remisión bajo requerimiento de la AEAT, no debe subsanar los errores relacionados con las validaciones de negocio de los registros enviados, ya que estos registros deben ser los que se han conservado en el sistema del obligado tributario en el momento de su generación.

# Estándares y requisitos.

## *Introducción.*

El contenido de un mensaje es un fichero XML. La codificación a utilizar debe ser UTF-8.

Un documento XML debe cumplir las reglas descritas en los diferentes esquemas, los cuales proporcionan normas respecto a formatos, obligatoriedad, etc. pero, en cualquier caso, la coherencia de los datos debe garantizarse en origen por quienes intervengan en la preparación y remisión de los datos.

Cada esquema está organizado en “Grupos de Datos” que contienen “Elementos de Datos”. Estos se han agrupado de modo que constituyen bloques lógicos, manteniendo una coherencia con el ámbito de cada esquema.

La remisión a través del servicio web podrá ser efectuada por el obligado tributario, un apoderado suyo a este trámite o un colaborador social, que deberá disponer de un certificado electrónico cualificado reconocido. Todos los NIFs se tienen que validar contra la “Base de Datos Centralizada de la AEAT”.

## *Estándares utilizados.*

El uso de servicios Web constituye la base de las buenas prácticas para desplegar servicios que posibiliten la interacción máquina-máquina, es decir, la automatización integral de un proceso en el que intervienen varios sistemas de información (el del ciudadano/empresa y el de la Agencia Tributaria).

Se pretende utilizar los estándares de facto para el desarrollo de servicios Web.

La estructura de los mensajes se basa en la creación de esquemas XML utilizando la recomendación W3C de 28-Octubre de 2004 en <http://www.w3.org/TR/xmlschema-0> y referenciada por el namespace <http://www.w3.org/2001/XMLSchema>

Con relación a SOAP se utilizará SOAP V1.1 disponible como NOTA W3C de 08-Mayo-2000 en: <http://www.w3.org/TR/2000/NOTE-SOAP-20000508/> y referenciado por el namespace <http://schemas.xmlsoap.org/soap/envelope/>

En SOAP-1.1 existen dos estilos para implementar servicios, modo “rpc” y modo “document”. En línea con las recomendaciones actuales se utilizará siempre el modo “document” (style = ”document”) sin ningún tipo de codificación (use = ”literal”). Es decir, el mensaje de entrada y salida estará descrito íntegramente por su respectivo esquema XML.

En la descripción de los servicios utilizaremos WSDL 1.1 disponible como NOTA W3C de 14-Marzo-2001 en: <http://www.w3.org/TR/2001/NOTE-wsdl-20010315> y referenciado por el namespace <http://schemas.xmlsoap.org/wsdl/>

## *Medio de envío.*

**Entorno**: Internet.

**Protocolo**: HTTPS.

**Mensajes**: Web Service con SOAP 1.1 modo Document.

**Certificado**: Las aplicaciones que envían información a los servicios web deberán autenticarse con certificado electrónico cualificado reconocido.

**Codificación**: UTF-8. La entrada es un XML que se debe adecuar a la especificación del siguiente esquema de entrada XSD.

# Consideraciones de diseño.

## *Comunicación de incidencias en el procesado de* peticiones.

En caso de incidencias al procesar el XML, serán comunicadas tal como se describen en el protocolo SOAP V1.1, es decir utilizando el elemento “Fault”.

A modo de resumen como respuesta a una remisión se pueden producir los siguientes casos:

|  |  |
| --- | --- |
| **Resultado en el lado cliente** | **Acción** |
| Se recibe una respuesta con XML esperado. (Puede ser de admisión o rechazo de la remisión) | OK. Mensaje procesado |
| Se recibe un respuesta con elemento “Fault” | Reenviar mensaje |

|  |  |
| --- | --- |
| y “faultcode” del tipo “soapenv:Server” |  |
| Se recibe una respuesta con elemento “Fault” y “faultcode” del tipo “soapenv:Client” | El mensaje no está bien formado o contiene información incorrecta. Compruebe el contenido del elemento “faultstring” para solucionar el problema antes de volver a enviar el mensaje. |
| No progresa la transmisión o bien no se  recibe un documento XML que responde a lo esperado | Reenviar mensaje |

# Diseño.

## *Estructura de los mensajes.*

##### Mensaje de remisión.

Contendrá una capa “SOAP” y en el “BODY” estarán los datos de la remisión.

##### Mensaje de respuesta.

Contendrá una capa “SOAP” y en el “BODY” estarán los datos de la respuesta.

## *Tipos de mensajes.*

### Definición del mensaje de remisión.

##### Fichero XML de “Remisión”:

* Cabecera.
* Lista de registros de facturación.

### Alta/Anulación de registros de facturación.

La estructura genérica de la remisión será la siguiente:

![](data:image/png;base64...)

La estructura del nodo *<RegistroAlta>* es la siguiente:

![](data:image/png;base64...)

La estructura del nodo *<RegistroAnulacion>* es la siguiente:

![](data:image/png;base64...)

##### Consulta de registros de facturación presentados (servicio solo disponible en remisión voluntaria «VERI\*FACTU»).

La estructura genérica de la remisión será la siguiente:

![](data:image/png;base64...)

##### Definición de los mensajes de respuesta.

**Fichero XML de “Respuesta” enviado por la AEAT.**

Cuando el mensaje de remisión se ha recibido correctamente en la AEAT y se está en disposición de procesar la información solicitada, se responderá con el fichero XML “Respuesta” con la información que corresponda. En este caso, estará compuesto de:

* Cabecera.
* Lista de registros aceptados y rechazados.

##### Respuesta Alta/Anulación de registros de facturación.

La estructura de la respuesta será la siguiente:

![](data:image/png;base64...)

##### Respuesta de la Consulta de registros de facturación presentados.

La estructura de la respuesta será la siguiente:

![](data:image/jpeg;base64...)

##### Fichero XML “SOAPFault”:

Cuando el mensaje de “Remisión” enviado por la empresa tiene errores en la validación a nivel de formato XML y/o en el contenido de los datos de la cabecera entonces se generará un mensaje de respuesta “SOAPFault” y se rechazará el envío completo.

##### Ejemplo de mensaje XML de respuesta “SOAPFault” informando de un error:

<?xml version="1.0" encoding="UTF-8"?>

<env:Envelope xmlns:env="<http://schemas.xmlsoap.org/soap/envelope/>">

<env:Body>

<env:Fault>

<faultcode>env:Client</faultcode>

<faultstring>Codigo[4104].El NIF del titular en la cabecera no está identificado.

NIF:iii. NOMBRE\_RAZON:xxx

</faultstring>

<detail>

<callstack>WSExcepcion [faultcode=null, detailMap=null, version=0,. </callstack>

</detail>

</env:Fault>

</env:Body>

</env:Envelope>

## *Especificación funcional del mensaje de remisión.*

Publicado en la sede electrónica de la Agencia Estatal de Administración Tributaria en un documento independiente, ubicado en el Portal de Desarrolladores:

[Enlace al Portal de desarrolladores de la AEAT](https://www.agenciatributaria.es/AEAT.desarrolladores/Desarrolladores/_menu_/Documentacion/Sistemas_Informaticos_de_Facturacion_y_Sistemas_VERI_FACTU/Sistemas_Informaticos_de_Facturacion_y_Sistemas_VERI_FACTU.html).

## *Especificación funcional de la consulta, por emisor y destinatario, de los registos de* facturación presentados (servicio solo disponible en remisión voluntaria «VERI\*FACTU»).

Se pueden consultar los registros previamente presentados por el emisor de la factura en remisión voluntaria (no en caso de remisión bajo requerimiento) filtrando obligatoriamente por el ejercicio y periodo de la factura (ejercicio/periodo de la fecha de operación o en su defecto de la fecha de expedición). Además, opcionalmente, podrá filtrar por otros campos, para permitir acotar con mayor precisión la relación de facturas obtenidas.

Se permite consultar tanto los propios registros de facturación que ha presentado el emisor (consulta realizada por el emisor de la factura) como los registros de facturación presentados por nuestro proveedor (consulta realizada por el destinatario de la factura). Si la consulta la realiza el emisor de los registros de facturación deberá filtrar en la cabecera por <ObligadoEmision> y si la consulta la realiza el destinatario deberá hacerlo por <Destinatario>.

Las consultas de registros de facturación informados se realizan por ejercicio/periodo “de imputación”, dato obtenido a partir de la fecha de operación o en su defecto de la fecha de expedición.

Las consultas responderán con un máximo de 10.000 registros. Si hay más datos pendientes en la respuesta, habrá que invocar al servicio de forma paginada (Ver 6.4.3 Consulta paginada) realizando nuevas consultas con la identificación del último registro obtenido, para obtener los siguientes registros ordenados por fecha de presentación.

### Consulta de los registos de facturación presentados

La estructura de la petición será la siguiente:

|  |  |  |
| --- | --- | --- |
| Leyenda | Rojo= | Campo obligatorio |
|  | Negro= | Campo opcional |

Campo de Selección

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
| **BLOQUE** | **DATOS/ AGRUPACIÓN** | **DATOS/ AGRUPACIÓN** | **DATOS/ AGRUPACIÓN** | **DESCRIPCIÓN** | **FORMATO / (LONGITUD) /**  **LISTA** |
| **Cabecera** | IDVersion |  |  | Identificación de la versión actual del esquema o estructura de información utilizada para la generación y conservación / remisión de los registros de facturación. Este campo forma parte  del detalle de las circunstancias de generación de los registros de facturación. | Alfanumérico (3) L15 |
| ObligadoEmision | NombreRazon |  | Nombre-razón social del obligado a expedir las facturas. | Alfanumérico (120) |
| NIF |  | NIF del obligado a expedir las facturas. | FormatoNIF (9) |
| Destinatario | NombreRazon |  | Nombre-razón social del destinatario, es decir, el cliente de la operación. | Alfanumérico (120) |
| NIF |  | Identificador del NIF del destinatario, es decir, el cliente de la operación. | FormatoNIF (9) |
| IndicadorRepresentante |  |  | Flag opcional que tendrá valor S si quien realiza la cosulta es el representante/asesor del obligado tributario. Permite, a quien realiza la cosulta, obtener los registros de facturación en los que figura como representante. Este flag solo se puede cumplimentar  cuando esté informado el obligado tributario en la consulta | Alfanumérico (1) L1C |
| **FiltroConsulta** | PeriodoImputacion | Ejercicio |  | Año de la fecha de la operación a consultar (obtenido del año de la fecha de operación o en su defecto de la fecha de expedición) | Númerico(4), formato YYYY |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
|  |  | Periodo |  | Mes de la fecha de la operación a consultar (obtenido del mes de la fecha de operación o en su defecto de la fecha de expedición) | Alfanumérico (2) L2C |
| NumSerieFactura |  |  | Nº Serie+Nº Factura que identifica al registro de facturacion. | Alfanumérico (60) |
| Contraparte | NombreRazon |  | Nombre-razón social de la contraparte del NIF de la cabecera que realiza la consulta.  Obligado emisor si la cosulta la realiza el Destinatario de los registros de facturacion.  Destinatario si la cosulta la realiza el Obligado dde los registros de facturacion.. | Alfanumérico (120) |
| NIF |  | Identificador del NIF de la contraparte del NIF de la cabecera que realiza la consulta.  Obligado emisor si la cosulta la realiza el Destinatario de los registros de facturacion.  Destinatario si la cosulta la realiza el Obligado dde los registros de facturacion.. | FormatoNIF (9) |
| IDOtro | CodigoPais |  | Alfanumérico (2)  (ISO 3166-1 alpha-  2 codes) |
| IDType | Clave para establecer el tipo de identificación, en el país de residencia, de la contraparte del NIF de la cabecera que realiza la consulta.  Obligado emisor si la cosulta la realiza el Destinatario de los registros de facturacion.  Destinatario si la cosulta la realiza el Obligado dde los registros de facturacion.. | Alfanumérico (2) L7 |
| ID | Número de identificación, en el país de residencia, de la contraparte del NIF de la cabecera que realiza la consulta. Obligado emisor si la cosulta la realiza el Destinatario de los registros de facturacion.  Destinatario si la cosulta la realiza el Obligado de los registros de facturacion.. | Alfanumérico (20) |
| FechaExpedicionFactura | FechaExpedicionFactura |  | Fecha de emisión del registro de facturacion | Fecha (dd-mm-yyyy) |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
|  |  | RangoFechaExpedicion | Desde | Fecha desde la que se consulta | Fecha (dd-mm-yyyy) |
| Hasta | Fecha hasta la que se consulta | Fecha (dd-mm-yyyy) |
| SistemaInformatico |  |  | Datos del sistema informático de facturación utilizado. Ver su diseño de bloque: «SistemaInformatico» en documento de diseño del alta/anulación |  |
| RefExterna |  |  | Dato adicional de contenido libre con el objetivo de que se pueda asociar opcionalmente información interna del sistema informático de facturación al registro de facturación | Alfanumérico (60) |
| ClavePaginacion | IDEmisorFactura |  | Número de identificación fiscal (NIF) del obligado a expedir la factura. (la ultima consultada) | FormatoNIF (9) |
| NumSerieFactura |  | Nº Serie+Nº Factura que identifica al registro de facturacion. (el ultimo consultado) | Alfanumérico (60) |
| FechaExpedicionFactura |  | Fecha de emisión del registro de facturacion (el ultimo consultado) | Fecha (dd-mm-yyyy) |
| DatosAdicionalesRespuesta | MostrarNombreRazonEmisor |  |  | Indicador que especifica si se quiere obtener en la respuesta el campo NombreRazonEmisor en la información del registro se facturacion. Si el Valor es S aumenta el tiempo de respuesta en la cosulta por detinatario.  Si no se informa este campo se entenderá que tiene valor “N”. | Alfanumérico(1)  Valores posibles: “S” o “N” |
| MostrarSistemaInformatico |  |  | Indicador que especifica si se quiere obtener en la respuesta el bloque SistemaInformatico en la información del registro se facturacion. Si el Valor es S aumenta el tiempo de respuesta en la cosulta. Si se consulta por Destinatario el valor del campo  MostrarSistemaInformatico debe ser 'N' o no estar cumplimentado | Alfanumérico(1)  Valores posibles: “S” o “N” |

### Respuesta de la consulta de los registos de facturación presentados

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
| **BLOQUE** | **DATOS/ AGRUPACIÓN** | **DATOS/**  **AGRUPA CIÓN** | **DATOS/ AGRUPACIÓN** | **DESCRIPCIÓN** | **FORMATO LONGITUD**  **LISTA** |
| Cabecera | IDVersion |  |  | Identificación de la versión del esquema utilizado para el intercambio de información | Alfanumérico(3) L15 |
| ObligadoEmision | NombreR  azon |  | Nombre-razón social del consultado. | Alfanumérico(120) |
| NIF |  | Nombre-razón social del obligado consultado. | FormatoNIF(9) |
| Destinatario | NombreR azon |  | Nombre-razón social del consultado. | Alfanumérico(120) |
| NIF |  | Identificador del NIF del consultado. | FormatoNIF(9) |
| IndicadorRepresentante |  |  | Indicador que indica si actua como representante o en nombre propio | Alfanumérico (1) L1C |
| PeriodoImputacio n | Ejercicio |  |  | Ejercicio consultado | Númerico(4), formato YYYY |
| Periodo |  |  | Periodo consultado | Alfanumérico (2) L2C |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
| IndicadorPaginaci on |  |  |  | Indica si hay más registros de facturación en la consulta realizada (Ver 6.4.3 Consulta paginada).  Si hay más datos pendientes, este campo tendrá valor “S” y se podrán realizar nuevas consultas indicando la identificación del último registro a partir de la cual se devolverán los siguientes registros ordenados por  fecha de presentación. | Alfanumérico(1) Valores posibles: “S” o “N” |
| ResultadoConsult a |  |  |  | Indica si hay registros de facturación para la consulta realizada. | Alfanumérico(8)  Valores posibles: “ConDatos” o “SinDatos” |
|  | Bloque que contiene todos los campos de una factura. Se obtendrán como máximo 10.000 facturas, es decir, este bloque puede repetirse  10.000 veces como máximo. | IDFactura |  |  | Bloque que contiene los campos que identifican al registros de facturación. |
|  | DatosRegi  stroFactur acion |  |  | Bloque que contiene los campos del registros de facturación registrado. |
| RespuestaConsult aFactuSistemaFac turacion | DatosPres entacion |  |  | Bloque que contiene los campos con información de la presentación realizada:  <NIFPresentador>  <TimestampPresentacion>  <IdPeticion> |
|  | EstadoRe gistro |  |  | Bloque que contiene los campos del estado del registro de facturación registrado:  <TimestampUltimaModificacion>  <EstadoRegistro>  <CodigoErrorRegistro>  <DescripcionErrorRegistro> |
| ClavePaginacion | IDEmisorFactura |  |  | Número de identificación fiscal (NIF) del obligado a expedir la factura. (la ultima consultada)  Solo se informa si IndicadorPaginacion=S | FormatoNIF (9) |
|  | NumSerieFactura |  |  | Nº Serie+Nº Factura que identifica al registro de facturación. (el ultimo consultado)  Solo se informa si IndicadorPaginacion=S | Alfanumérico (60) |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
|  | FechaExpedicionFactura |  |  | Fecha de emisión del registro de facturación (el ultimo consultada)  Solo se informa si IndicadorPaginacion=S | Fecha (dd-mm-yyyy) |

### Consulta paginada

Si al realizar una consulta hay más datos pendientes en la respuesta (el servicio de consulta responderá con un máximo de 10.000 registros) habrá que invocar al servicio de forma paginada. En este caso el campo <IndicadorPaginacion> de la respuesta tendrá valor “S” y

<ClavePaginacion> estará relleno con el último registro de facturación de la respuesta, para informar de que se deben realizar nuevas consultas si desea obtener el resto de registros.

Para obtener el resto de registros pendientes deberá enviar una nueva petición de consulta cumplimentando el campo <ClavePaginacion> con la clave del registro que haya devuelto la respuesta de la consulta anterior a partir del que se obtendrán el resto. Si no se informa el bloque

<ClavePaginacion> la consulta responderá con los 10.000 primeros registros ordenados por fecha de presentación.

### Respuesta de alta y anulación de registros de facturación.

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
| **BLOQUE** | **DATOS/ AGRUPACIÓN** | **DATOS/ AGRUPACIÓN** | **D A T O**  **S** | **DESCRIPCIÓN** | **FORMATO LONGITUD LISTA** |
| CSV |  |  |  | Código seguro de verificación asociado a la remisión enviada.  IMPORTANTE: El CSV debe ser almacenado por el SIF en el momento de alta, no podrá ser recuperado a través de consultas posteriores. | Alfanumérico(16) |
| DatosPresentaci on | NIFPresentador |  |  | NIF del presentador. | FormatoNIF(9) |
| TimestampPresentacion |  |  | Timestamp asociado a la remisión enviada, en huso horario de los servidores de la AEAT. | DateTime. Formato: YYYY-MM-  DDThh:mm:ssTZD (ej: 2024-01-01T19:20:30+01:00  ) (ISO 8601) |
| Cabecera |  |  |  | Cabecera equivalente a la enviada en la remisión de alta  / anulación. |  |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
| TiempoEsperaE nvio |  |  |  | Segundos de espera entre envíos.  Para poder realizar el siguiente envío, el sistema informático deberá esperar a que transcurran  <TiempoEsperaEnvio> segundos desde el anterior envío o deberá esperar a tener acumulados un número de registros de facturación igual al límite establecido en el diseño de registro para cada envío, la circunstancia que  ocurra primero | Numérico |
| EstadoEnvío |  |  |  | Campo que especifica si el conjunto de registros de facturación enviados han sido admitidos correctamente,  han sido rechazados, o se han aceptado de forma parcial. | Alfanumérico(20) L18 |
| RespuestaLinea (0..1000) | IDFactura |  |  | Identificador del registro de facturación especificado en la remisión de alta/anulación. |  |
| Operacion | TipoOperacion |  | Tipo de operación realizada que puede ser “Alta” o “Anulacion” | Alfanumérico(9) L22 |
| Subsanacion |  | Indicador de “Subsanacion” especificado en la remisión de alta | Alfanumérico (1) L4 |
| RechazoPrevio |  | Indicador de “RechazoPrevio” especificado en la remisión de alta/anulación | Alfanumérico (1) L17 |
| SinRegistroPrevio |  | Indicador de “SinRegistroPrevio” especificado en la remisión de anulación | Alfanumérico (1) L4 |

|  |  |  |  |  |  |
| --- | --- | --- | --- | --- | --- |
|  | RefExterna |  |  | Dato adicional de contenido libre especificado en la remisión de alta/anulación. | Alfanumérico(60) |
| EstadoRegistro |  |  | Campo que especifica si el registro de facturación ha  sido registrado correctamente, ha sido rechazado, o se trata de un caso en el ha sido registrado, pero con errores  . | Alfanumérico(18) L19 |
| CodigoErrorRegistro |  |  | Código que identifica el tipo de error producido en el registro de facturación. | Alfanumérico(5) L20 |
| DescripcionErrorRegistro |  |  | Descripción del error producido en un registro de facturación. | Alfanumérico(500) |
| RegistroDuplicado | IdPeticionRegistroDupli cado |  | IdPeticion asociado al registro almacenado previamente en el sistema. Solo se suministra si el registro enviado es rechazado por estar duplicado | Alfanumérico(20) |
| EstadoRegistroDuplica do |  | Estado del registro almacenado previamente en el  sistema. Los estados posibles son: Correcta, AceptadaConErrores y Anulada.   * Solo se suministra si el registro enviado es rechazado por estar duplicada. | Alfanumérico(18) L21 |
| CodigoErrorRegistro |  | Código del error del registro almacenado previamente en  el sistema | Alfanumérico(5) L20 |
| DescripcionErrorRegist ro |  | Descripción detallada del error del registro duplicado  registrado previamente en el sistema | Alfanumérico(500) |

#### *Mecanismo de control de flujo.*

Viene especificado en el artículo 16.2 de la orden:

1. Los sistemas informáticos «VERI\*FACTU» deberán implementar un mecanismo de control de flujo basado en el tiempo de espera entre envíos, el cual tomará inicialmente el valor de 60 segundos, y en el número máximo de registros admitidos en cada envío.

Los mensajes de respuesta de la Agencia Estatal de Administración Tributaria informarán sobre el valor de este parámetro, el cual deberá ser tenido en cuenta para el siguiente envío.

El número máximo de registros a remitir en cada envío queda determinado por el diseño de registro incluido en el apartado 2.2 del anexo. El funcionamiento será el siguiente:

* 1. El sistema informático realiza el envío del primer conjunto de registros de facturación a la Agencia Estatal de Administración Tributaria.
  2. La Agencia Estatal de Administración Tributaria devuelve, entre otros datos, un valor actualizado del parámetro de tiempo de espera «t» entre envíos.
  3. Para poder realizar el siguiente envío, el sistema informático deberá esperar a que transcurran «t» segundos desde el anterior envío o deberá esperar a tener acumulados un número de registros de facturación igual al límite establecido en el diseño de registro para cada envío, la circunstancia que ocurra primero.
  4. El sistema informático realiza un nuevo envío cumpliendo con lo establecido en la letra c). En la respuesta puede recibir una nueva actualización del valor del parámetro «t».

Ejemplo de respuesta con el parámetro de tiempo de espera de 60 segundos entre envíos:

<sf:TiempoEsperaEnvio>60</sf:TiempoEsperaEnvio>

## *Valores permitidos en campos de tipo lista.*

### Alta, anulación y consulta de registros de facturación.

Se recoge en el apartado 6 del anexo de la orden. Además, se encuentra publicado en la sede electrónica de la Agencia Estatal de Administración Tributaria en un documento independiente, ubicado en el Portal de Desarrolladores:

[Enlace al portal de desarrolladores de la AEAT.](https://www.agenciatributaria.es/AEAT.desarrolladores/Desarrolladores/_menu_/Documentacion/IVA/Sistemas_Informaticos_de_Facturacion_y_Sistemas_VERI_FACTU/Sistemas_Informaticos_de_Facturacion_y_Sistemas_VERI_FACTU.html)

### Consulta de registros de facturación.

L1C  Indicador Representante

|  |  |
| --- | --- |
| **VALORES** | **DESCRIPCIÓN** |
| **S** | Sí |

L2C  Periodo

|  |  |
| --- | --- |
| **VALORES** | **DESCRIPCIÓN** |
| **01** | Enero |
| **02** | Febrero |
| **03** | Marzo |

|  |  |
| --- | --- |
| **04** | Abril |
| **05** | Mayo |
| **06** | Junio |
| **07** | Julio |
| **08** | Agosto |
| **09** | Septiembre |
| **10** | Octubre |
| **11** | Noviembre |
| **12** | Diciembre |

### Respuesta de alta y anulación de registros de facturación.

L18 Estado global del envío (respuesta al envío).

|  |  |
| --- | --- |
| **VALORES** | **DESCRIPCIÓN** |
| **Correcto** | Todos los registros de facturación de la remisión tienen estado “Correcto”. |
| **ParcialmenteCorrecto** | Algunos registros de la remisión tienen estado “Incorrecto” o “AceptadoConErrores”. |
| **Incorrecto** | Todos los registros de la remisión tienen estado “Incorrecto”. |

L19 Estado del envío del registro (respuesta al envío).

Campo **<EstadoRegistro>** de la respuesta al envío de alta y anulación. Especifica si el registro enviado se valida correctamente y es por tanto admitido y registrado en el sistema.

|  |  |
| --- | --- |
| **VALORES** | **DESCRIPCIÓN** |
| **Correcto** | El registro de facturación es totalmente correcto y se registra en el sistema. |
| **AceptadoConErrores** | El registro de facturación tiene errores que no provocan su rechazo. Se registra en el sistema. |
| **Incorrecto** | El registro de facturación tiene errores que provocan su rechazo. No se registra en el sistema. |

L20 Código de error de registro.

La lista completa de errores se encuentra en el documento de validaciones.

L21 Estado del registro de facturación de la factura (obtenido como respuesta cuando se rechaza el alta de un registro por duplicado).

Campo ***<EstadoRegistroDuplicado>*** de la respuesta a un alta cuando el registro se rechaza por duplicado. Especifica el estado del registro almacenado previamente en el sistema.

|  |  |
| --- | --- |
| **VALORES** | **DESCRIPCIÓN** |
| **Correcta** | El registro de facturación registrado previamente es correcto. |
| **AceptadaConErrores** | El registro de facturación registrado previamente tiene algunos errores |
| **Anulada** | El registro de facturación registrado previamente ha sido anulado |

|  |  |
| --- | --- |
|  | mediante una operación de anulación |

L22 Tipo de operación realizada en el registro de facturación

|  |  |
| --- | --- |
| **VALORES** | **DESCRIPCIÓN** |
| **Alta** | La operación realizada ha sido un alta |
| **Anulacion** | La operación realizada ha sido una anulación |

## *Remisión voluntaria y bajo requerimiento.*

En la remisión voluntaria los obligados tributarios remitirán inmediatamente a la Administración tributaria, de forma automática y segura por medios electrónicos, todos los registros de facturación generados en sus sistemas informáticos.

Bajo requerimiento de la Administración tributaria el obligado tributario suministrará los registros de facturación conservados, mediante medios electrónicos, a la sede electrónica de dicha Administración tributaria.

Existe un único formato de registro de facturación para remisión voluntaria por parte sistemas que emiten facturas verificables y ante un requerimiento. Por tanto, el esquema XSD es único para los dos casos.

Aunque el esquema XSD es común, existén URLs diferentes para la remisión voluntaria y ante requerimiento. Además, son distintos sistemas de gestión en la AEAT, sin compartición de los registros de facturación remitidos entre ellos.

![](data:image/jpeg;base64...)La definición de los servicios web para la remisión voluntaria y ante requerimiento se encuentra en el archivo WSDL en la siguiente dirección: <https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl>

## *Tratamiento de cadenas de texto en campos XML*

No se recomienda comenzar y/o finalizar con espacios en blanco las cadenas de texto presentes en los registros XML, en cualquier caso, se eliminarán los espacios en blanco al inicio y al final de cada valor. Este proceso se reflejará en la respuesta de las peticiones y se almacenará de esta manera en los sistemas de la AEAT.

Por ejemplo, si el campo NumSerieFactura contiene la siguiente información:

<NumSerieFactura> 12345678 / G33 </NumSerieFactura> se almacenará y se responderá con el valor "12345678 / G33".

## *Valores permitidos en campos numéricos.*

Para valores numéricos, los ceros por la izquierda no deberán emplearse (por ejemplo, 01 ó 001 ó 01230 serían incorrectos; en su lugar debería ponerse 1, 1 y 1230 respectivamente). Tras el punto de separación decimal, los ceros por la derecha sólo podrán ser usados para indicar la precisión decimal (por ejemplo: 12345.7 es lo mismo que 12345.70; y 12345 es lo mismo que 12345.0 y que 12345.00).

(Nota: dentro del formato fecha, los campos numéricos que expresen cada uno de los componentes de la misma sí deben llevar ceros por la izquierda hasta completar el número de dígitos requerido, como, por ejemplo: 02-07-2014 (y no 2-7-2014).

## *Aclaración sobre escapado de caracteres especiales.*

En caso de que fuera necesario consignar en un valor de un elemento XML algunos de los siguientes caracteres, se escaparán con las entidades xml siguientes:

|  |  |
| --- | --- |
| **Carácter** | **Carácter escapado** |
| & | &amp; |
| < | &lt; |

# Anexo I: Definición de servicios y esquemas (entorno de PRUEBAS).

Contiene la definición de los servicios y esquemas de la versión 1.0.

## *Definición de servicios.*

La definición de los servicios (WSDL) se encuentra en la siguiente dirección:

<https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl>

## *Esquemas de Entrada*

El esquema de los mensajes de entrada definidos se ha incluido en los siguientes archivos:

* “SuministroInformacion.xsd”. Contiene la definición de tipos comunes: <https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroInformacion.xsd>
* “SuministroLR.xsd”. Esquema de las operaciones (Alta y Anulación) establecidos:

<https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroLR.xsd>

* ConsultaLR.xsd. Esquema de la operación de consulta. <https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/ConsultaLR.xsd>

## *Esquemas de Salida.*

El esquema de los mensajes de respuesta definidos se ha incluido en los siguientes archivos:

* “RespuestaSuministro.xsd”. Esquema de respuesta de las operaciones (Alta y Anulación) establecidos: <https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaSuministro.xsd>
* RespuestaConsultaLR.xsd. Esquema de respuesta de las operaciones de consulta. <https://prewww2.aeat.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaConsultaLR.xsd>

# Anexo I: Definición de servicios y esquemas (entorno de PRODUCCIÓN).

Contiene la definición de los servicios y esquemas de la versión 1.0.

## *Definición de servicios.*

La definición de los servicios (WSDL) se encuentra en la siguiente dirección:

<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SistemaFacturacion.wsdl>

## *Esquemas de Entrada*

El esquema de los mensajes de entrada definidos se ha incluido en los siguientes archivos:

* “SuministroInformacion.xsd”. Contiene la definición de tipos comunes: <https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroInformacion.xsd>
* “SuministroLR.xsd”. Esquema de las operaciones (Alta y Anulación) establecidos:

<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/SuministroLR.xsd>

* ConsultaLR.xsd. Esquema de la operación de consulta. <https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/ConsultaLR.xsd>

## *Esquemas de Salida.*

El esquema de los mensajes de respuesta definidos se ha incluido en los siguientes archivos:

* “RespuestaSuministro.xsd”. Esquema de respuesta de las operaciones (Alta y Anulación) establecidos: <https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaSuministro.xsd>
* RespuestaConsultaLR.xsd. Esquema de respuesta de las operaciones de consulta. <https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tikeV1.0/cont/ws/RespuestaConsultaLR.xsd>

# Anexo II: Operativa de remisión voluntaria «VERI\*FACTU»

En la remisión voluntaria los obligados tributarios remitirán inmediatamente a la Administración tributaria, de forma automática y segura por medios electrónicos, todos los registros de facturación generados en sus sistemas informáticos.

## *Operativa: Alta de un registro de facturación.*

### Alta inicial (“normal”) del registro de facturación.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |
| ALTA | * Alta inicial ("normal") del registro de facturación. * Es el alta habitual de un registro de facturación. | * No informar <Subs anacion> o informarlo con valor N * No informar <RechazoPrevio> o   informarlo con valor N | El registro de facturación no debe existir previamente en SIF del  obligado / AEAT. | Alta del registro de  facturación con los nuevos datos. |

En el registro de alta se incluirá la propia huella (según las especificaciones dadas en el documento de huella de la sede electrónica de la AEAT) del registro de facturación. Como siempre, el encadenamiento debe ser con el registro de facturación inmediatamente anterior, por orden cronológico de generación de registros de facturación en el SIF.

#### *Ejemplo mensaje XML de alta inicial (“normal”) del registro de facturación.*

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

### Subsanación del registro de facturación.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |
| ALTA DE SUBSANACIÓN | * Alta para la subsanación de un registro de facturación ya generado/remitido anteriormente. * Es la subsanación habitual de un registro de facturación cuando no se exige la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de Facturación). | * <Subsanacion>=S * No informar <RechazoPrevio> o informarlo con valor N | El registro de facturación debe existir previamente en SIF del obligado / AEAT. | Con él se deja constancia de los nuevos datos que deben ser tenidos en cuenta. |

Cuando sea necesario subsanar la información que contiene un registro de facturación que ha sido enviado y con estado “Aceptado” o “AceptadoConErrores” por la AEAT, siempre y cuando no se trate de una causa que exija la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de Facturación) ni se anule la factura, se deberá generar un nuevo registro (“de subsanación”), con la misma clave de registro original que se quiere subsanar, ya con los datos completos y correctos, que será remitido dentro de un nuevo fichero o mensaje de envío a la AEAT.

El registro original no se puede modificar, por lo que permanecerá inalterado.

En el nuevo registro subsanado se incluirá la propia huella (según se indica en el documento de huella) del registro de facturación. Como siempre, el encadenamiento debe ser con el registro de facturación inmediatamente anterior (sea de alta o de anulación), por orden cronológico de generación de registros de facturación en el SIF.

#### *Ejemplo mensaje XML de subsanación del registro de facturación.*

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:Subsanacion>S</sum1:Subsanacion>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

### Alta, tras un rechazo previo del registro de facturación de alta inicial (y que, por tanto, no existe aún en la AEAT).

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |
|  | · Alta por rechazo del registro de facturación de alta |  |  |  |
| ALTA POR RECHAZO | inicial (y que, por tanto, no existe aún en la AEAT).  · Es la subsanación de datos de un registro de  facturación de alta inicial, cuando no se exige la emisión de una factura rectificativa (u otro  mecanismo contemplado en el Reglamento de Facturación). | * <Subs anacion>=S * <RechazoPrevio>=X | * La clave única del registro de facturación no debe existir previamente en la AEAT. * El alta previa del registro de facturación fue rechazada. | Alta del registro de  facturación con los nuevos datos. |

Cuando sea necesario subsanar la información de un registro de facturación de alta inicial, que ha sido rechazado (y que, por tanto, no existe aún en la AEAT), siempre y cuando no se trate de una causa que exija la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de Facturación) ni se anule la factura, se deberá generar un nuevo registro “de subsanación” de alta inicial por rechazo, ya con los datos completos y correctos, que será enviado a la AEAT.

El registro original no se puede modificar, por lo que permanecerá inalterado.

En el nuevo registro subsanado se incluirá la propia huella (según las especificaciones dadas en el documento de huella de la sede electrónica de la AEAT) del registro de facturación. Como siempre, el encadenamiento debe ser con el registro de facturación inmediatamente anterior (sea de alta o de anulación), por orden cronológico de generación de registros de facturación en el SIF.

#### *Ejemplo mensaje XML de alta, tras un rechazo previo del registro de facturación de alta inicial (y* que, por tanto, no existe aún en la AEAT).

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:Subsanacion>S</sum1:Subsanacion>

<sum1:RechazoPrevio>X</sum1:RechazoPrevio>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

## *Operativa: Anulación de un registro de facturación.*

### Anulación del registro de facturación.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| ANULACIÓN | * Anulación de registro de facturación ya generado/remitido. * Es la anulación habitual de un registro de facturación cuando no se exige la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de Facturación). | * No informar **<SinRegistroPrevio>** o informarlo con valor N * No informar **<RechazoPrevio>** o informarlo con valor N | * El registro de facturación debe existir previamente en SIF del obligado / AEAT. * El registro a anular puede ser de alta o de anulación (en cuyo caso, deja constancia de los nuevos datos a tener en cuenta). | Anula el registro de facturación registrado dejando los nuevos datos. |

Cuando se trate de una operación incorrecta (casos excluidos de poder emitir una factura rectificativa u otro mecanismo contemplado en el Reglamento de Facturación), como por ejemplo que se haya expedido una factura por error cuando no ha habido una auténtica venta, deberá realizarse además una ANULACIÓN de la operación original procediendo a generar un registro de facturación de anulación”), con la misma clave de registro original que se quiere anular.

En el registro de anulación se incluirá la propia huella (según las especificaciones dadas en el documento de huella de la sede electrónica de la AEAT) del registro de facturación. Como cualquier registro de facturación, el registro de anulación irá encadenado al registro de facturación inmediatamente anterior (sea de alta o de anulación), por orden cronológico de generación de registros de facturación en el SIF.

#### *Ejemplo mensaje XML de anulación del registro de facturación.*

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAnulacion>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFacturaAnulada>AAAA</sum1:IDEmisorFacturaAnulada>

<sum1:NumSerieFacturaAnulada>12345</sum1:NumSerieFacturaAnulada>

<sum1:FechaExpedicionFacturaAnulada>13-09-2024</sum1:FechaExpedicionFacturaAnulada>

</sum1:IDFactura>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAnulacion>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

### Anulación, tras un rechazo previo del registro de facturación de anulación.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |
|  |  |  | · La clave única del registro de |  |
| ANULACIÓN POR RECHAZO | · Anul ación (tras un rechazo previo) del registro de facturación cuando el registro de facturación que se quiere anular está registrado en la AEAT. | * No informar **<SinRegistroPrevio>**   o informarlo con valor N   * **<RechazoPrevio>**=S | facturación debe existir previamente en la AEAT.  · La anulación previa del registro  de facturación fue rechazada. | Anul a el registro de facturación registrado  dejando los nuevos datos. |

Cuando sea necesario subsanar la información de un registro de facturación de anulación que ha sido rechazado, se deberá generar un nuevo registro “de subsanación de anulación por rechazo”, con la misma clave de registro original que se quiere anular, ya con los datos correctos, que será remitido dentro de un nuevo fichero o mensaje de envío a la AEAT.

El registro original de anulación no se puede modificar, por lo que permanecerá inalterado.

En el nuevo registro de anulación subsanado se incluirá la propia huella (según las especificaciones dadas en el documento de huella de la sede electrónica de la AEAT) del registro de facturación. Como siempre, el encadenamiento debe ser con el registro de facturación inmediatamente anterior (sea de alta o de anulación), por orden cronológico de generación de registros de facturación en el SIF.

#### *Ejemplo mensaje XML de anulación, tras un rechazo previo del registro de facturación de* anulación.

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAnulacion>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFacturaAnulada>AAAA</sum1:IDEmisorFacturaAnulada>

<sum1:NumSerieFacturaAnulada>12345</sum1:NumSerieFacturaAnulada>

<sum1:FechaExpedicionFacturaAnulada>13-09-2024</sum1:FechaExpedicionFacturaAnulada>

</sum1:IDFactura>

<sum1:RechazoPrevio>S</sum1:RechazoPrevio>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAnulacion>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

### Anulación cuando el registro de facturación que se quiere anular NO está registrado en la AEAT.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |
| ANULACIÓN SIN REGISTRO PREVIO | ·Anulación del registro de facturación cuando el registro de facturacion que se quiere anular NO está registrado en la AEAT | * **<SinRegistroPrevio>**=S * No informar **<RechazoPrevio>** o informarlo con valor N | ·La clave unica del registro de facturación no debe existir previamente en la AEAT. | Alta con estado anulado del registro de facturación con los nuevos datos |

Cuando sea necesario anular un registro de facturación, pero el registro de facturacion que se quiere anular NO está registrado en la AEAT, se deberá generar un nuevo registro de anulación sin resgistro previo, que será remitido dentro de un nuevo fichero o mensaje de envío a la AEAT.

Este sería el caso, por ejemplo, cuando el registro previo que se quiere anular no ha sido enviado a la AEAT por no ser un sistema que emite facturas verificables en ese momento. Otro ejemplo sería por haber sido rechazado el registro de alta (por contener errores no admisibles), pero finalmente se determina que se trata de una factura a anular (por darse las circunstancias que así lo permiten) y se remite directamente el correspondiente registro de anulación

En el registro de anulación se incluirá la propia huella (según las especificaciones dadas en el documento de huella de la sede electrónica de la AEAT) del registro de facturación. Como cualquier registro de facturación, el registro de anulación irá encadenado al registro de facturación inmediatamente anterior (sea de alta o de anulación), por orden cronológico de generación de registros de facturación en el SIF

#### *Ejemplo mensaje XML de anulación cuando el registro de facturación que se quiere anular NO* está registrado en la AEAT.

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAnulacion>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFacturaAnulada>AAAA</sum1:IDEmisorFacturaAnulada>

<sum1:NumSerieFacturaAnulada>12345</sum1:NumSerieFacturaAnulada>

<sum1:FechaExpedicionFacturaAnulada>13-09-2024</sum1:FechaExpedicionFacturaAnulada>

</sum1:IDFactura>

<sum1:SinRegistroPrevio>S</sum1:SinRegistroPrevio>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAnulacion>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

## *Operativa habitual de remisión agrupada de registros de facturación.*

### Ejemplo de mensaje XML que incluye tres registros de facturación (dos registros de facturación de alta y uno de anulación).

##### Fichero XML de entrada con 3 registros de facturación:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-02-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-02-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior44</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<!--You have a CHOICE of the next 2 items at this level-->

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-02-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

<sum:RegistroFactura>

<sum1:RegistroAnulacion>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFacturaAnulada>AAAA</sum1:IDEmisorFacturaAnulada>

<sum1:NumSerieFacturaAnulada>12311</sum1:NumSerieFacturaAnulada>

<sum1:FechaExpedicionFacturaAnulada>13-02-2024</sum1:FechaExpedicionFacturaAnulada>

</sum1:IDFactura>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-02-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior12345</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-02-13T19:20:40+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAnulacion>

</sum:RegistroFactura>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12346</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-02-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12311</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-02-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnteriorAnulacion12311</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<!--You have a CHOICE of the next 2 items at this level-->

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-02-13T19:20:50+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

# Anexo III: Operativa de remisión de registros de facturación para responder a un requerimiento de la AEAT «No VERI\*FACTU».

Bajo requerimiento de la Administración tributaria el obligado tributario suministrará los registros de facturación **conservados**, mediante medios electrónicos, a la sede electrónica de dicha Administración tributaria

Los registros de facturación, enviados bajo el requerimiento de la AEAT, se darán de alta como un registro nuevo en la AEAT, independientemente de si son registros de alta o anulación.

Las validaciones de negocio de la Administración tributaria no provocarán el rechazo de los registros de facturación. Simplemente se marcarán como errores admisibles para no rechazar los registros de facturación conservados en el sistema del obligado tributario. Las únicas validaciones que provocan el rechazo de la factura son las de identificación de <NIF> o <IdOtro>

No se deben subsanar los errores relacionados con las validaciones de negocio de los registros enviados, ya que estos registros deben ser los que se han conservado en el sistema del obligado tributario en el momento de su generación.

En todos los envíos bajo requerimiento se debe suministrar obligatoriamente, en la cabecera, el campo <RefRequerimiento> donde se informa de la referencia del requerimiento de la AEAT.

En el último envío también se deberá informar, en la cabecera, la finalización de la remisión de registros de facturación del requerimiento marcando el campo <FinRequerimiento> con el valor “S”. Especialmente útil cuando se realice un envío o remisión múltiple para dejar constancia de que se trata del último envío.

## *Operativa: Alta de un registro de facturación.*

### Alta inicial (“normal”) del registro de facturación.

|  |  |  |  |
| --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Consecuencias** |

|  |  |  |  |
| --- | --- | --- | --- |
| ALTA | · Alta inicial (“normal”) del registro de facturación. | · No informar **<Subsanacion>** o informarlo con valor N  · No informar **<RechazoPrevio>** o informarlo con valor N | ·Alta del registro de facturación con los nuevos datos |

En el registro de alta se incluirá la propia huella (según las especificaciones dadas en el documento de huella de la sede electrónica de la AEAT) del registro de facturación. Como siempre, el encadenamiento debe ser con el registro de facturación inmediatamente anterior, por orden cronológico de generación de registros de facturación en el SIF.

#### *Ejemplo mensaje XML de alta inicial (“normal”) del registro de facturación.*

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

<sum1:RemisionRequerimiento>

<sum1:RefRequerimiento>XXXXXXXXXXXXXXXXXX</sum1:RefRequerimiento>

</sum1:RemisionRequerimiento>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

### Subsanación del registro de facturación.

|  |  |  |  |
| --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Consecuencias** |
| ALTA DE SUBSANACIÓN | Alta para la subsanación de un registro de facturación ya generado/remitido anteriormente.  · Es la subsanación habitual de un registro de facturación cuando no se exige la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de  Facturación). | · **<Subsanacion>** = S  · No informar **<RechazoPrevio>** o informarlo con valor N | Con él se deja constancia de los nuevos datos que deben ser tenidos en cuenta. |

En el caso de que se haya realizado una subsanación (si no ha exigido la emisión de una factura rectificativa) de un registro de facturación en el SIF del obligado tributario, se habrá generado un nuevo registro (“de subsanación”), con la misma clave de registro original que se quiere subsanar con los datos completos y correctos.

Este registro de subsanación **conservado** en el SIF deberá ser enviado, aunque no suponga la modificación del registro original, que permanecerá inalterado.

En ningún caso se utilizará un registro de subsanación para subsanar los errores relacionados con las validaciones de negocio de los registros enviados, ya que estos registros deben ser los que se han conservado en el sistema del obligado tributario en el momento de su generación.

#### *Ejemplo mensaje XML de subsanación del registro de facturación.*

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd" xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

<sum1:RemisionRequerimiento>

<sum1:RefRequerimiento>XXXXXXXXXXXXXXXXXX</sum1:RefRequerimiento>

</sum1:RemisionRequerimiento>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAlta>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>12345</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

</sum1:IDFactura>

<sum1:NombreRazonEmisor>XXXXX</sum1:NombreRazonEmisor>

<sum1:Subsanacion>S</sum1:Subsanacion>

<sum1:TipoFactura>F1</sum1:TipoFactura>

<sum1:DescripcionOperacion>Descripc</sum1:DescripcionOperacion>

<sum1:Destinatarios>

<sum1:IDDestinatario>

<sum1:NombreRazon>YYYY</sum1:NombreRazon>

<sum1:NIF>BBBB</sum1:NIF>

</sum1:IDDestinatario>

</sum1:Destinatarios>

<sum1:Desglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>4</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>10</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>0.4</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

<sum1:DetalleDesglose>

<sum1:ClaveRegimen>01</sum1:ClaveRegimen>

<sum1:CalificacionOperacion>S1</sum1:CalificacionOperacion>

<sum1:TipoImpositivo>21</sum1:TipoImpositivo>

<sum1:BaseImponibleOimporteNoSujeto>100</sum1:BaseImponibleOimporteNoSujeto>

<sum1:CuotaRepercutida>21</sum1:CuotaRepercutida>

</sum1:DetalleDesglose>

</sum1:Desglose>

<sum1:CuotaTotal>21.4</sum1:CuotaTotal>

<sum1:ImporteTotal>131.4</sum1:ImporteTotal>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAlta>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

## *Operativa: Anulación de un registro de facturación.*

### Anulación del registro de facturación.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Operación** | **Descripción** | **Operativa** | **Condiciones** | **Consecuencias** |

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| ANULACIÓN | * Anulación de registro de facturación ya generado/remitido. * Es la anulación habitual de un registro de facturación cuando no se exige la emisión de una factura rectificativa (u otro mecanismo contemplado en el Reglamento de Facturación). | * No informar **<SinRegistroPrevio>** o informarlo con valor N * No informar **<RechazoPrevio>** o informarlo con valor N | · El registro a anular puede ser de alta o de anulación (en cuyo caso, deja constancia de los nuevos datos a tener en cuenta). | Crea un nuevo registro de facturación dejando ambos datos(anulación y alta). |

En el caso de que se haya realizado una anulación (si no ha exigido la emisión de una factura rectificativa) de un registro de facturación en el SIF del obligado tributario, se habrá generado un nuevo registro (“de anulación”), con la misma clave de registro original que se quiere anular.

Este registro de anulación **conservado** en el SIF deberá ser enviado, aunque no suponga la modificación del registro original, que permanecerá inalterado.

#### *Ejemplo mensaje XML de anulación del registro de facturación.*

##### Fichero XML de entrada:

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroL> R.xsd"

xmlns:sum1="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/Suministro> Informacion.xsd" xmlns:xd="[http://www.w3.org/2000/09/xmldsig#](http://www.w3.org/2000/09/xmldsig)">

<soapenv:Header/>

<soapenv:Body>

<sum:RegFactuSistemaFacturacion>

<sum:Cabecera>

<sum1:ObligadoEmision>

<sum1:NombreRazon>XXXXX</sum1:NombreRazon>

<sum1:NIF>AAAA</sum1:NIF>

</sum1:ObligadoEmision>

<sum1:RemisionRequerimiento>

<sum1:RefRequerimiento>XXXXXXXXXXXXXXXXXX</sum1:RefRequerimiento>

</sum1:RemisionRequerimiento>

</sum:Cabecera>

<sum:RegistroFactura>

<sum1:RegistroAnulacion>

<sum1:IDVersion>1.0</sum1:IDVersion>

<sum1:IDFactura>

<sum1:IDEmisorFacturaAnulada>AAAA</sum1:IDEmisorFacturaAnulada>

<sum1:NumSerieFacturaAnulada>12345</sum1:NumSerieFacturaAnulada>

<sum1:FechaExpedicionFacturaAnulada>13-09-2024</sum1:FechaExpedicionFacturaAnulada>

</sum1:IDFactura>

<sum1:Encadenamiento>

<sum1:RegistroAnterior>

<sum1:IDEmisorFactura>AAAA</sum1:IDEmisorFactura>

<sum1:NumSerieFactura>44</sum1:NumSerieFactura>

<sum1:FechaExpedicionFactura>13-09-2024</sum1:FechaExpedicionFactura>

<sum1:Huella>HuellaRegistroAnterior</sum1:Huella>

</sum1:RegistroAnterior>

</sum1:Encadenamiento>

<sum1:SistemaInformatico>

<sum1:NombreRazon>SSSS</sum1:NombreRazon>

<sum1:NIF>NNNN</sum1:NIF>

<sum1:NombreSistemaInformatico>NombreSistemaInformatico</sum1:NombreSistemaInformatico>

<sum1:IdSistemaInformatico>77</sum1:IdSistemaInformatico>

<sum1:Version>1.0.03</sum1:Version>

<sum1:NumeroInstalacion>383</sum1:NumeroInstalacion>

<sum1:TipoUsoPosibleSoloVerifactu>N</sum1:TipoUsoPosibleSoloVerifactu>

<sum1:TipoUsoPosibleMultiOT>S</sum1:TipoUsoPosibleMultiOT>

<sum1:IndicadorMultiplesOT>S</sum1:IndicadorMultiplesOT>

</sum1:SistemaInformatico>

<sum1:FechaHoraHusoGenRegistro>2024-09-13T19:20:30+01:00</sum1:FechaHoraHusoGenRegistro>

<sum1:TipoHuella>01</sum1:TipoHuella>

<sum1:Huella>Huella</sum1:Huella>

</sum1:RegistroAnulacion>

</sum:RegistroFactura>

</sum:RegFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

# Anexo IV: Operativa de consulta de información presentada (servicio solo disponible en remisión voluntaria «VERI\*FACTU»)

Consulta de la información presentada por el emisor de la factura. La cosulta la puede realizar tanto el emisor del registro de facturación como el destinatario (servicio solo disponible en remisión voluntaria «VERI\*FACTU»).

## *Operativa: Consulta del emisor de los registros de facturación para obtener los registros* presentados.

### Consulta de registros de facturación presentados previamente ordenados por fecha de presentación

Consulta de registros de facturación presentados previamente. El servicio responderá con un máximo de 10.000 registros de facturación ordenados por fecha de presentación

#### *Ejemplo mensaje XML de consulta de registros de facturación presentados previamente.* Consulta del emisor del registro de facturación.

##### XML de entrada

<?xml version="1.0" encoding="UTF-8"?>

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:con="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd>">

<soapenv:Header/>

<soapenv:Body>

<con:ConsultaFactuSistemaFacturacion>

<con:Cabecera>

<sum:IDVersion>1.0</sum:IDVersion>

<sum:ObligadoEmision>

<sum:NombreRazon>EMPRESAXXXX</sum:NombreRazon>

<sum:NIF>XXXXXXXXX</sum:NIF>

</sum:ObligadoEmision>

</con:Cabecera>

<con:FiltroConsulta>

<con:PeriodoImputacion>

<sum:Ejercicio>2024</sum:Ejercicio>

<sum:Periodo>11</sum:Periodo>

</con:PeriodoImputacion>

</con:FiltroConsulta>

</con:ConsultaFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

##### XML de respuesta

<?xml version="1.0" encoding="UTF-8"?>

<env:Envelope xmlns:env="<http://schemas.xmlsoap.org/soap/envelope/>">

<env:Header/>

<env:Body Id="Body">

<tikLRRC:RespuestaConsultaFactuSistemaFacturacion xmlns:tikLRRC="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/RespuestaConsultaLR.xsd>" xmlns:tik="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd>">

<tikLRRC:Cabecera>

<tik:IDVersion>1.0</tik:IDVersion>

<tik:ObligadoEmision>

<tik:NombreRazon>EMPRESAXXXX</tik:NombreRazon>

<tik:NIF>XXXXXXXXX</tik:NIF>

</tik:ObligadoEmision>

</tikLRRC:Cabecera>

<tikLRRC:PeriodoImputacion>

<tikLRRC:Ejercicio>2024</tikLRRC:Ejercicio>

<tikLRRC:Periodo>11</tikLRRC:Periodo>

</tikLRRC:PeriodoImputacion>

<tikLRRC:IndicadorPaginacion>N</tikLRRC:IndicadorPaginacion>

<tikLRRC:ResultadoConsulta>ConDatos</tikLRRC:ResultadoConsulta>

<tikLRRC:RegistroRespuestaConsultaFactuSistemaFacturacion>

<tikLRRC:IDFactura>

<tik:IDEmisorFactura>XXXXXXXXX</tik:IDEmisorFactura>

<tik:NumSerieFactura>88</tik:NumSerieFactura>

<tik:FechaExpedicionFactura>27-11-2024</tik:FechaExpedicionFactura>

</tikLRRC:IDFactura>

<tikLRRC:DatosRegistroFacturacion>

<tikLRRC:TipoFactura>F1</tikLRRC:TipoFactura>

<tikLRRC:DescripcionOperacion>Servicios de reparación</tikLRRC:DescripcionOperacion>

<tikLRRC:Destinatarios>

<tikLRRC:IDDestinatario>

<tik:NombreRazon>yyyyyyyyyyyyy</tik:NombreRazon>

<tik:NIF>A84532509</tik:NIF>

</tikLRRC:IDDestinatario>

</tikLRRC:Destinatarios>

<tikLRRC:Cupon>N</tikLRRC:Cupon>

<tikLRRC:Desglose>

<tik:DetalleDesglose>

<tik:Impuesto>01</tik:Impuesto>

<tik:ClaveRegimen>01</tik:ClaveRegimen>

<tik:CalificacionOperacion>S1</tik:CalificacionOperacion>

<tik:TipoImpositivo>4</tik:TipoImpositivo>

<tik:BaseImponibleOimporteNoSujeto>1044.03</tik:BaseImponibleOimporteNoSujeto>

<tik:CuotaRepercutida>41.76</tik:CuotaRepercutida>

</tik:DetalleDesglose>

</tikLRRC:Desglose>

<tikLRRC:CuotaTotal>41.76</tikLRRC:CuotaTotal>

<tikLRRC:ImporteTotal>16976.37</tikLRRC:ImporteTotal>

<tikLRRC:Encadenamiento>

<tikLRRC:RegistroAnterior>

<tik:IDEmisorFactura>XXXXXXXXX</tik:IDEmisorFactura>

<tik:NumSerieFactura>87</tik:NumSerieFactura>

<tik:FechaExpedicionFactura>27-11-2024</tik:FechaExpedicionFactura>

<tik:Huella>3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60</tik:Huella>

</tikLRRC:RegistroAnterior>

</tikLRRC:Encadenamiento>

<tikLRRC:FechaHoraHusoGenRegistro>2024-11-27T11:54:10+01:00</tikLRRC:FechaHoraHusoGenRegistro>

<tikLRRC:TipoHuella>01</tikLRRC:TipoHuella>

<tikLRRC:Huella>DAA7F72EEDC1AE8A294B7A011EC4A1EC2BE0E4DDB79AF3758377F8D61F38FE6B</tikLRRC:Huella>

</tikLRRC:DatosRegistroFacturacion>

<tikLRRC:DatosPresentacion>

<tik:NIFPresentador>89890002E</tik:NIFPresentador>

<tik:TimestampPresentacion>2024-11-13T11:54:10+01:00</tik:TimestampPresentacion>

<tik:IdPeticion>20241113115410360242</tik:IdPeticion>

</tikLRRC:DatosPresentacion>

<tikLRRC:EstadoRegistro>

<tikLRRC:TimestampUltimaModificacion>2024-11-27T11:54:10+01:00</tikLRRC:TimestampUltimaModificacion>

<tikLRRC:EstadoRegistro>Correcta</tikLRRC:EstadoRegistro>

</tikLRRC:EstadoRegistro>

</tikLRRC:RegistroRespuestaConsultaFactuSistemaFacturacion>

</tikLRRC:RespuestaConsultaFactuSistemaFacturacion >

</env:Body>

</env:Envelope>

#### *Ejemplo mensaje XML de consulta de registros de facturación presentados previamente filtrando* por ejercicio, periodo y NIF de la contraparte. Consulta del emisor del registro de facturación.

<?xml version="1.0" encoding="UTF-8"?>

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:con="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd>">

<soapenv:Header/>

<soapenv:Body>

<con:ConsultaFactuSistemaFacturacion>

<con:Cabecera>

<sum:IDVersion>1.0</sum:IDVersion>

<sum:ObligadoEmision>

<sum:NombreRazon>EMPRESAXXXX</sum:NombreRazon>

<sum:NIF>XXXXXXXXX</sum:NIF>

</sum:ObligadoEmision>

</con:Cabecera>

<con:FiltroConsulta>

<con:PeriodoImputacion>

<sum:Ejercicio>2024</sum:Ejercicio>

<sum:Periodo>11</sum:Periodo>

</con:PeriodoImputacion>

<con:Contraparte>

<sum:NombreRazon>EMPRESAYYYY</sum:NombreRazon>

<sum:NIF>A84532509</sum:NIF>

</con:Contraparte>

</con:FiltroConsulta>

</con:ConsultaFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

#### *Ejemplo mensaje XML de consulta de registros de facturación presentados previamente filtrando* por ejercicio, periodo y un rago de fecha de expedición. Consulta del emisor del registro de facturación.

<?xml version="1.0" encoding="UTF-8"?>

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:con="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd>">

<soapenv:Header/>

<soapenv:Body>

<con:ConsultaFactuSistemaFacturacion>

<con:Cabecera>

<sum:IDVersion>1.0</sum:IDVersion>

<sum:ObligadoEmision>

<sum:NombreRazon>EMPRESAXXXX</sum:NombreRazon>

<sum:NIF>XXXXXXXXX</sum:NIF>

</sum:ObligadoEmision>

</con:Cabecera>

<con:FiltroConsulta>

<con:PeriodoImputacion>

<sum:Ejercicio>2024</sum:Ejercicio>

<sum:Periodo>11</sum:Periodo>

</con:PeriodoImputacion>

<con:FechaExpedicionFactura>

<sum:RangoFechaExpedicion>

<sum:Desde>02-11-2024</sum:Desde>

<sum:Hasta>13-11-2024</sum:Hasta>

</sum:RangoFechaExpedicion>

</con:FechaExpedicionFactura>

</con:FiltroConsulta>

</con:ConsultaFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>

### Consulta del destinatario (cliente) de los registros de facturación para obtener los registros presentados por su proveedor.

El destinatario del resgistro de facturación puede consultar las facturas presentados por su proveedor.

Si al realizar una consulta de registros de facturación, se supera el tope de 10.000 registros en la respuesta, habrá que realizar nuevas consultas con la identificación del último registro obtenido (informando el bloque <ClavePaginacion> de la petición) para obtener el resto de regsistros.

#### *Ejemplo mensaje XML de consulta paginada de registros de facturación presentados* previamente. Consulta del destinatario del registro de facturación.

##### XML de entrada

<?xml version="1.0" encoding="UTF-8"?>

<soapenv:Envelope xmlns:soapenv="<http://schemas.xmlsoap.org/soap/envelope/>" xmlns:con="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/ConsultaLR.xsd>" xmlns:sum="<https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd>">

<soapenv:Header/>

<soapenv:Body>

<con:ConsultaFactuSistemaFacturacion>

<con:Cabecera>

<sum:IDVersion>1.0</sum:IDVersion>

<sum:Destinatario>

<sum:NombreRazon>EMPRESAXXXX</sum:NombreRazon>

<sum:NIF>XXXXXXXXX</sum:NIF>

</sum:Destinatario>

</con:Cabecera>

<con:FiltroConsulta>

<con:PeriodoImputacion>

<sum:Ejercicio>2024</sum:Ejercicio>

<sum:Periodo>11</sum:Periodo>

</con:PeriodoImputacion>

<con:ClavePaginacion>

<sum:IDEmisorFactura>XXXXXXXXX</sum:IDEmisorFactura>

<sum:NumSerieFactura>10000</sum:NumSerieFactura>

<sum:FechaExpedicionFactura>28-11-2024</sum:FechaExpedicionFactura>

</con:ClavePaginacion>

</con:FiltroConsulta>

</con:ConsultaFactuSistemaFacturacion>

</soapenv:Body>

</soapenv:Envelope>