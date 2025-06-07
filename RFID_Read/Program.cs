using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace RFID_Read
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            // 요청된 어셈블리 이름에서 DLL 파일 이름 추출
            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            // 현재 어셈블리 리소스 중 해당 DLL 이름으로 끝나는 항목 찾기
            var resources = thisAssembly
                .GetManifestResourceNames()
                .Where(s => s.EndsWith(name, StringComparison.OrdinalIgnoreCase));

            var enumerable = resources.ToList();

            if (!enumerable.Any())
                return null; // 리소스에 해당 DLL이 없으면 null 반환

            // 첫 번째 일치하는 리소스 이름 사용
            var resourceName = enumerable.First();

            // 해당 리소스를 스트림으로 읽기
            var stream = thisAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;

            // 스트림 내용을 byte[]로 읽어 메모리에 로딩
            var assembly = new byte[stream.Length];
            stream.Read(assembly, 0, assembly.Length);

            return Assembly.Load(assembly); // 메모리에서 어셈블리 로딩
        }
    }
}
