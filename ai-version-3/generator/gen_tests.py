#!/usr/bin/env python3
# generator/gen_tests.py

import yaml
import argparse
import os
from pathlib import Path
from typing import Dict, List, Any

# ---------------------------------------------------------
# 1. ШАБЛОНЫ КОДА
# ---------------------------------------------------------
# Двойные фигурные скобки {{}} экранируют литералы C# кода
TEST_FILE_TEMPLATE = """// =============================================================
// AUTO-GENERATED TESTS. DO NOT EDIT MANUALLY.
// Source: {spec_source}
// Generator: gen_tests.py v1.0
// =============================================================
using System;
using NUnit.Framework;
using Module.Core; 
using {namespace};

namespace Module.Tests
{{
    [TestFixture]
    [Description("Автоматически сгенерированные тесты для {module}")]
    public class {module}Tests
    {{
        private IJsonPathExtractor _sut;

        [SetUp]
        public void SetUp()
        {{
            _sut = new JsonPathExtractor();
        }}

{methods}
    }}
}}
"""

TEST_METHOD_TEMPLATE = """    [Test]
    [Description("{case} | Требования: {req}")]
    [TestCase({inputs})]
    public void Test_{method}_{case_name}(string json, string path)
    {{
        // Arrange: Pre={pre}
        // Expected: {expected}

        // Act
        var result = _sut.ExtractValue(json, path);

        // Assert: Post={post}
        Assert.That(result, Is.EqualTo("{expected}").Or.Null, "Mismatch on {case}");
    }}
"""

# ---------------------------------------------------------
# 2. ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
# ---------------------------------------------------------
def load_spec(spec_path: str) -> Dict[str, Any]:
    """Безопасная загрузка YAML-спецификации."""
    with open(spec_path, "r", encoding="utf-8") as f:
        return yaml.safe_load(f)

def format_csharp_input(value: Any) -> str:
    """Преобразует значение из YAML в литерал C# с правильным экранированием."""
    if value is None:
        return "null"
    if isinstance(value, str):
        # Экранируем обратные слеши и кавычки для C#
        escaped = value.replace("\\", "\\\\").replace("\"", "\\\"")
        return f'"{escaped}"'
    if isinstance(value, bool):
        return "true" if value else "false"
    return str(value)

# ---------------------------------------------------------
# 3. ГЕНЕРАЦИЯ КОДА
# ---------------------------------------------------------
def generate_method_tests(method_data: Dict[str, Any]) -> List[str]:
    """Генерирует тесты для одного метода."""
    case_blocks = []
    req_trace = ", ".join(
        method_data.get("req_functional", []) + 
        method_data.get("req_non_functional", [])
    )
    
    for eq_class in method_data.get("equivalence_classes", []):
        # Формируем список входных параметров для [TestCase]
        inputs_str = ",".join(format_csharp_input(inp) for inp in eq_class["inputs"])
        
        # Создаём безопасное имя для метода
        case_name = eq_class["case"].replace(" ", "_").replace("(", "").replace(")", "").replace("-", "")
        
        # Добавляем блок теста
        case_blocks.append(
            TEST_METHOD_TEMPLATE.format(
                method=method_data["name"],
                case=eq_class["case"],
                case_name=case_name,
                inputs=inputs_str,
                req=req_trace,
                pre=method_data["pre"],
                post=method_data["post"],
                expected=eq_class["expected"]
            )
        )
    
    return case_blocks

def render_and_save(spec: Dict[str, Any], config: Dict[str, Any]) -> None:
    """Собирает полный файл тестов и сохраняет на диск."""
    module = spec["module"]
    namespace = config.get("target_namespace", "Module.Core")
    
    # Генерируем все тесты
    methods_code = []
    for method in spec["methods"]:
        methods_code.extend(generate_method_tests(method))
    
    # Рендерим файл
    file_content = TEST_FILE_TEMPLATE.format(
        spec_source=config.get("spec_path", "N/A"),
        module=module,
        namespace=namespace,
        methods="\n".join(methods_code)
    )
    
    # Сохраняем
    out_dir = Path(config.get("output_dir", "tests/Module.Tests"))
    out_dir.mkdir(parents=True, exist_ok=True)
    output_file = out_dir / f"{module}Tests.Generated.cs"
    output_file.write_text(file_content, encoding="utf-8")
    
    print(f"[✓] Сгенерирован: {output_file}")
    print(f"   Методов: {len(spec['methods'])}")
    print(f"   Тестов: {sum(len(m.get('equivalence_classes', [])) for m in spec['methods'])}")

# ---------------------------------------------------------
# 4. ТОЧКА ВХОДА
# ---------------------------------------------------------
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="C# NUnit Test Generator from YAML Spec")
    parser.add_argument("--config", default="config.yaml", help="Путь к config.yaml")
    args = parser.parse_args()
    
    print("[*] Загрузка конфигурации...")
    with open(args.config, "r", encoding="utf-8") as f:
        config = yaml.safe_load(f)
    
    print(f"[*] Загрузка спецификации: {config['spec_path']}...")
    spec_data = load_spec(config["spec_path"])
    
    print("[*] Генерация C# тестов...")
    render_and_save(spec_data, config)
    
    print("[✓] Готово.")
