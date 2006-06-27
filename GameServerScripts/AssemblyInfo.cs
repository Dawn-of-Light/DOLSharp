/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System.Reflection;
//
// Allgemeine Informationen über eine Assembly werden über folgende Attribute 
// gesteuert. Ändern Sie diese Attributswerte, um die Informationen zu modifizieren,
// die mit einer Assembly verknüpft sind.
//
[assembly : AssemblyTitle("")]
[assembly : AssemblyDescription("")]
[assembly : AssemblyConfiguration("")]
[assembly : AssemblyCompany("")]
[assembly : AssemblyProduct("")]
[assembly : AssemblyCopyright("")]
[assembly : AssemblyTrademark("")]
[assembly : AssemblyCulture("")]

//
// Versionsinformationen für eine Assembly bestehen aus folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion 
//      Buildnummer
//      Revision
//
// Sie können alle Werte oder die standardmäßige Revision und Buildnummer 
// mit '*' angeben:

[assembly : AssemblyVersion("1.0.*")]

//
// Um die Assembly zu signieren, müssen Sie einen Schlüssel angeben. Weitere Informationen 
// über die Assemblysignierung finden Sie in der Microsoft .NET Framework-Dokumentation.
//
// Mit den folgenden Attributen können Sie festlegen, welcher Schlüssel für die Signierung verwendet wird. 
//
// Hinweise: 
//   (*) Wenn kein Schlüssel angegeben ist, wird die Assembly nicht signiert.
//   (*) KeyName verweist auf einen Schlüssel, der im CSP (Crypto Service
//       Provider) auf Ihrem Computer installiert wurde. KeyFile verweist auf eine Datei, die einen
//       Schlüssel enthält.
//   (*) Wenn die Werte für KeyFile und KeyName angegeben werden, 
//       werden folgende Vorgänge ausgeführt:
//       (1) Wenn KeyName im CSP gefunden wird, wird dieser Schlüssel verwendet.
//       (2) Wenn KeyName nicht vorhanden ist und KeyFile vorhanden ist, 
//           wird der Schlüssel in KeyFile im CSP installiert und verwendet.
//   (*) Um eine KeyFile zu erstellen, können Sie das Programm sn.exe (Strong Name) verwenden.
//       Wenn KeyFile angegeben wird, muss der Pfad von KeyFile
//       relativ zum Projektausgabeverzeichnis sein:
//       %Project Directory%\obj\<configuration>. Wenn sich KeyFile z.B.
//       im Projektverzeichnis befindet, geben Sie das AssemblyKeyFile-Attribut 
//       wie folgt an: [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Das verzögern der Signierung ist eine erweiterte Option. Weitere Informationen finden Sie in der
//       Microsoft .NET Framework-Dokumentation.
//
[assembly : AssemblyDelaySign(false)]
[assembly : AssemblyKeyFile("")]
[assembly : AssemblyKeyName("")]