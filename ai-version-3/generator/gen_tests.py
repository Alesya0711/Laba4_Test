#!/usr/bin/env python3
import yaml, argparse, os
from pathlib import Path
from typing import Dict, List, Any

TEST_FILE_TEMPLATE = """// AUTO-GENERATED. DO NOT EDIT MANUALLY.
using System;
using NUnit.Framework;
using Lab.Interfaces;  
using {namespace;           
 
namespace Module.Tests
{{
    [TestFixture]
    [Description("Сгенерированные тесты для {module}")]           
    public class {module}Tests        
    {{
        private IJsonPathExtractor _sut;           

        [SetUp]
        public void SetUp() => _sut = new JsonPathExtractor();

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
        // Act
        var result = _sut.ExtractValue(json, path);
        // Assert: Post={post}
        Assert.That(result, Is.EqualTo("{expected}").Or.Null, "Mismatch on {case}");
    }}
"""

def format_csharp_input(val):
    if val is None: return "null"
    if isinstance(val, str): return f'"{val}"'
    return str(val)

def generate(spec, config):
    module = spec["module"]
    ns = config.get("target_namespace", "Lab.Implementations.GenCode1")
    methods_code = []
    for m in spec["methods"]:
        for ec in m.get("equivalence_classes", []):
            inputs = ",".join(format_csharp_input(i) for i in ec["inputs"])
            case_name = ec["case"].replace(" ","_").replace(".","").replace("-","")
            req = ",".join(m.get("req_functional",[])+m.get("req_non_functional",[]))
            methods_code.append(TEST_METHOD_TEMPLATE.format(
                method=m["name"], case=ec["case"], case_name=case_name,
                inputs=inputs, req=req, pre=m["pre"], post=m["post"], expected=ec["expected"]
            ))
    out_dir = Path(config["output_dir"])
    out_dir.mkdir(parents=True, exist_ok=True)
    file_path = out_dir / f"{module}Tests.Generated.cs"
    file_path.write_text(TEST_FILE_TEMPLATE.format(module=module, namespace=ns, methods="\n".join(methods_code)), encoding="utf-8")
    print(f"[✓] {file_path} | Тестов: {len(methods_code)}")

if __name__ == "__main__":
    p = argparse.ArgumentParser(); p.add_argument("--config", default="config.yaml")
    args = p.parse_args()
    with open(args.config, "r", encoding="utf-8") as f: config = yaml.safe_load(f)
    with open(config["spec_path"], "r", encoding="utf-8") as f: spec = yaml.safe_load(f)
    generate(spec, config)
