Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Globalization
Imports System.Resources
Imports System.Windows

' Informações gerais sobre um assembly são controladas através do seguinte 
' conjunto de atributos. Altere o valor destes atributos para modificar a informação
' associada a um assembly.

' Revise os valores dos atributos do assembly

<Assembly: AssemblyTitle("DCGF_Tool")> 
<Assembly: AssemblyDescription("")> 
<Assembly: AssemblyCompany("")> 
<Assembly: AssemblyProduct("DCGF_Tool")> 
<Assembly: AssemblyCopyright("Copyright ©  2014")> 
<Assembly: AssemblyTrademark("")> 
<Assembly: ComVisible(false)>

'Para iniciar a compilação de aplicações localizáveis, configure 
'<UICulture>CulturaQueVoceEstaProgramando</UICulture> no seu arquivo .vbproj
'dentro de uma <PropertyGroup>.  Por exemplo, se você está utilizando inglês US 
'nos seus arquivos fonte, configure a <UICulture> para "en-US".  Depois descomente o
'atributo NeutralResourceLanguage abaixo.  Atualize o "en-US" na linha
'abaixo para coincidir a configuração de UICulture no arquivo de projeto.

'<Assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)> 


'O atributo ThemeInfo descreve onde encontrar temas específicos e dicionários de recursos genéricos.
'1o parâmetro: onde dicionários específicos de temas se encontram
'(usado se algum recurso não é encontrado na página, 
' ou dicionários de recursos de aplicação)

'2o parâmetro: onde os dicionários de recursos genéricos se encontram
'(usado se algum recurso não é encontrado na página, 
'aplicação, e qualquer dicionário de recursos de tema específico)
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>



'O GUID a seguir é para o ID da typelib se este projeto for exposto para COM
<Assembly: Guid("acbab8d6-1d5b-4e59-9bec-32bb6302a961")> 

' Informação de versão para um assembly consiste nos quatro valores a seguir:
'
'      Versão Principal
'      Versão Secundária 
'      Número da Versão
'      Revisão
'
' É possível especificar todos os valores ou usar o padrão de números da Compilação e de Revisão 
' utilizando o '*' como mostrado abaixo:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("1.0.0.0")> 
<Assembly: AssemblyFileVersion("1.0.0.0")> 
